namespace Applr.RestApi.Models;

/// <summary>
/// One pattern as the filler reads it. The match type travels with the text
/// because 'literal' means a phrase to find, not a regex to run: without it the
/// filler cannot tell a label containing ( ) or ? from a malformed regex.
/// </summary>
public sealed class ProfilePatternResponse
{
    public string Pattern { get; set; } = string.Empty;

    /// <summary>'regex' or 'literal'.</summary>
    public string MatchType { get; set; } = "regex";
}
