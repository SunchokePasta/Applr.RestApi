namespace Applr.RestApi.Models;

/// <summary>
/// The single shape every failed request returns, written by
/// ExceptionHandlingMiddleware. Applr.API returns the same shape, and
/// the desktop client knows how to read it -- keep the three property
/// names identical across both APIs or the client falls back to a
/// generic message.
/// </summary>
public sealed class ApiErrorResponse
{
    /// <summary>Safe to show a user. Never an exception message.</summary>
    public string Message { get; init; } = string.Empty;

    /// <summary>Matches the [Reference] tag on the logged line.</summary>
    public string Reference { get; init; } = string.Empty;

    public int Status { get; init; }
}
