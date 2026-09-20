namespace Applr.RestApi.Models;

/// <summary>
/// Body for POST profile/fields/{fieldKey}/patterns -- a new wording for a
/// field that already exists, which is what the resolver returns for verdict
/// 'existing'. No value is involved: the data is already held, only the label
/// was unfamiliar.
/// </summary>
public sealed class CreateFieldPatternRequest
{
    public string Pattern { get; set; } = string.Empty;

    public string MatchType { get; set; } = "literal";

    public string Domain { get; set; } = string.Empty;

    public string Source { get; set; } = "llm";

    public string Status { get; set; } = "pending";

    public decimal? Confidence { get; set; }

    public string? LlmRunId { get; set; }
}
