using Applr.RestApi.Data;
using Applr.RestApi.Middleware;
using Applr.RestApi.Repositories;
using Applr.RestApi.Services;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Events;

var builder = WebApplication.CreateBuilder(args);

// Console + a rolling daily file under ./logs, anchored to the app's own
// content root rather than the process's working directory -- same
// reasoning as TrackrScraper/Scraper.py's logging setup: whatever starts
// this (Task Scheduler, IIS, a plain `dotnet run` from a different cwd)
// shouldn't change where the log file ends up.
var logDirectory = Path.Combine(builder.Environment.ContentRootPath, "logs");
Directory.CreateDirectory(logDirectory);

// SourceContext is what tells you whether a line came from a controller,
// a repository or EF itself. Without it every line looks the same.
const string LogTemplate =
    "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {SourceContext} {Message:lj}{NewLine}{Exception}";

builder.Host.UseSerilog((context, configuration) => configuration
    .MinimumLevel.Information()

    // ASP.NET writes four lines per request ("Request starting",
    // "Executing endpoint", "Route matched", "Request finished").
    // UseSerilogRequestLogging below replaces all four with one summary
    // line that includes the elapsed time, so the originals are noise.
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)

    // EF logs every generated SQL statement at Information. That is what
    // turned log-20260914.txt into 138KB of SELECT statements, and it
    // buries the lines that actually matter. Warning here still shows
    // command *failures*; set it back to Information temporarily when
    // you're debugging a query.
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", LogEventLevel.Warning)

    .Enrich.FromLogContext()
    .WriteTo.Console(outputTemplate: LogTemplate)
    .WriteTo.File(
        Path.Combine(logDirectory, "log-.txt"),
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 14,
        outputTemplate: LogTemplate,
        shared: true));

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var connectionString = builder.Configuration.GetConnectionString("JobFinderDb")
    ?? throw new InvalidOperationException(
        "Missing ConnectionStrings:JobFinderDb -- set it in appsettings.Development.json " +
        "(gitignored) or an environment variable before running.");

builder.Services.AddDbContext<ApplrDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

// One repository per table -- see RawJobRepository/CompanyRepository/
// JobRepository, none of which join across tables. JobSyncService is
// the one piece that reads more than one of them (to diff RawJobs
// against Jobs) and is where the promote-to-Unreviewed logic lives.
builder.Services.AddScoped<IRawJobRepository, RawJobRepository>();
builder.Services.AddScoped<ICompanyRepository, CompanyRepository>();
builder.Services.AddScoped<IJobRepository, JobRepository>();
builder.Services.AddScoped<IJobSyncService, JobSyncService>();

// The three profile tables share one repository: a field is meaningless
// without its patterns and a value cannot exist without its field, so they
// are queried together rather than one-per-table like the job tables.
builder.Services.AddScoped<IProfileRepository, ProfileRepository>();

var app = builder.Build();

// First in the pipeline on purpose: anything registered after this is
// wrapped by it, so a throw anywhere downstream becomes a logged
// ApiErrorResponse instead of an unhandled 500 with an empty body.
app.UseMiddleware<ExceptionHandlingMiddleware>();

// One line per request: method, path, status, elapsed ms. Replaces the
// four ASP.NET lines suppressed above.
app.UseSerilogRequestLogging();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
