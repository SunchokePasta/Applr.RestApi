namespace Applr.RestApi.Diagnostics;

/// <summary>
/// Short, human-quotable id attached to a single failure. The same value
/// goes into the log line and into the response body, so "it broke and
/// said 7f3a9c21" is enough to find the exact stack trace in
/// logs/log-yyyyMMdd.txt.
///
/// Deliberately not the ASP.NET TraceIdentifier: that one looks like
/// "0HN8K2Q9R4V1B:00000003", which nobody is going to read back
/// correctly. The trace identifier is logged alongside this anyway, so
/// nothing is lost.
/// </summary>
public static class ErrorReference
{
    public static string New() => Guid.NewGuid().ToString("N")[..8];
}
