using System.Data.Common;
using System.Text.Json;
using Applr.RestApi.Diagnostics;
using Applr.RestApi.Models;
using Microsoft.EntityFrameworkCore;

namespace Applr.RestApi.Middleware;

/// <summary>
/// One place where every unhandled exception in this API is caught,
/// logged with a reference id, and turned into an
/// <see cref="ApiErrorResponse"/>. Registered first in the pipeline (see
/// Program.cs) so it wraps model binding, routing, controllers,
/// repositories and EF alike.
///
/// The point is that controllers and repositories no longer need their
/// own try/catch. A repository can let a DbException escape and it lands
/// here as a 503 with the stack trace in the log. Only catch something
/// lower down when the catch changes behaviour -- JobSyncService
/// swallowing a duplicate-key DbUpdateException is a real example: that
/// one is a no-op for a single row, not a failed request.
/// </summary>
public sealed class ExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<ExceptionHandlingMiddleware> logger)
{
    private static readonly JsonSerializerOptions JsonOptions =
        new(JsonSerializerDefaults.Web);

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (OperationCanceledException)
            when (context.RequestAborted.IsCancellationRequested)
        {
            // The caller hung up mid-request (closed the app, navigated
            // away, timed out its own client). Nothing is broken here,
            // so this is Information, not Error -- otherwise every
            // abandoned request looks like a fault in the log.
            logger.LogInformation(
                "{Method} {Path} was cancelled by the caller.",
                context.Request.Method,
                context.Request.Path);

            if (!context.Response.HasStarted)
            {
                // 499: nginx's "client closed request". Nothing is sent
                // back -- there is nobody left to read it.
                context.Response.StatusCode = 499;
            }
        }
        catch (Exception exception)
        {
            await WriteErrorAsync(context, exception);
        }
    }

    private async Task WriteErrorAsync(HttpContext context, Exception exception)
    {
        var (status, message) = Describe(exception);
        var reference = ErrorReference.New();

        logger.Log(
            status >= StatusCodes.Status500InternalServerError
                ? LogLevel.Error
                : LogLevel.Warning,
            exception,
            "[{Reference}] {Method} {Path} failed with {StatusCode}. TraceId {TraceId}.",
            reference,
            context.Request.Method,
            context.Request.Path,
            status,
            context.TraceIdentifier);

        if (context.Response.HasStarted)
        {
            // Headers are already on the wire (a partially streamed
            // response). We can't rewrite the status, and appending JSON
            // would corrupt the body -- the log line above is all we get.
            logger.LogWarning(
                "[{Reference}] The response had already started; no error body was sent.",
                reference);

            return;
        }

        context.Response.Clear();
        context.Response.StatusCode = status;
        context.Response.ContentType = "application/json";

        await context.Response.WriteAsync(
            JsonSerializer.Serialize(
                new ApiErrorResponse
                {
                    Message = message,
                    Reference = reference,
                    Status = status
                },
                JsonOptions),
            context.RequestAborted);
    }

    /// <summary>
    /// Exception type -> (status code, message a human may read). The
    /// messages here are intentionally vague about internals: the
    /// specifics live in the log line the reference id points at.
    /// </summary>
    private static (int Status, string Message) Describe(Exception exception) =>
        exception switch
        {
            // SQLSTATE class 23 is "integrity constraint violation" in
            // any SQL dialect -- for us, almost always the UNIQUE index
            // on jobs.raw_job_id or companies.name.
            DbUpdateException { InnerException: DbException { SqlState: not null } inner }
                when inner.SqlState!.StartsWith("23", StringComparison.Ordinal) =>
                (StatusCodes.Status409Conflict,
                    "That record already exists."),

            DbUpdateException =>
                (StatusCodes.Status500InternalServerError,
                    "The database rejected the write."),

            // Covers MySqlException: server down, bad credentials,
            // connection pool exhausted, dropped mid-query.
            DbException =>
                (StatusCodes.Status503ServiceUnavailable,
                    "The database is not reachable right now."),

            // A cancellation that isn't the caller hanging up (handled
            // above) is a timeout on our side.
            OperationCanceledException =>
                (StatusCodes.Status504GatewayTimeout,
                    "The request took too long and was stopped."),

            // Bad route values or a malformed body that got past model
            // binding -- the caller's problem, not ours.
            ArgumentException or FormatException =>
                (StatusCodes.Status400BadRequest,
                    "The request could not be understood."),

            // Thrown by Program.cs for missing configuration, and by
            // anything that finds itself in an impossible state.
            InvalidOperationException =>
                (StatusCodes.Status500InternalServerError,
                    "The API is misconfigured."),

            _ => (StatusCodes.Status500InternalServerError,
                    "Something went wrong.")
        };
}
