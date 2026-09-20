namespace Applr.RestApi.Models;

/// <summary>
/// Body for POST profile/fields -- a field the matcher has never seen, which
/// is what the resolver returns for verdict 'new'. The pattern that revealed
/// it is created in the same call, because a field with no way of being
/// recognised is dead weight.
/// </summary>
public sealed class CreateProfileFieldRequest
{
    public string FieldKey { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Omit to append after the existing fields. Supply one only to place a
    /// field deliberately -- ordering decides which pattern claims an input
    /// first, so an arbitrary value can break matching that already works.
    /// </summary>
    public int? MatchOrder { get; set; }

    public string? MirrorsKey { get; set; }

    public bool IsSensitive { get; set; }

    /// <summary>The label that prompted this, stored as the field's first pattern.</summary>
    public string Pattern { get; set; } = string.Empty;

    public string MatchType { get; set; } = "literal";

    public string Domain { get; set; } = string.Empty;

    public string Source { get; set; } = "llm";

    public string Status { get; set; } = "pending";

    public decimal? Confidence { get; set; }

    public string? LlmRunId { get; set; }
}
