namespace Applr.RestApi.Models;

/// <summary>
/// A field plus the patterns the filler should match with, in the order they
/// must be applied. Shaped so ApplrFiller can rebuild its matcher from one
/// call rather than fetching patterns per field.
/// </summary>
public sealed class ProfileFieldResponse
{
    public string FieldKey { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public int MatchOrder { get; set; }

    public string? MirrorsKey { get; set; }

    public bool IsSensitive { get; set; }

    public List<ProfilePatternResponse> Patterns { get; set; } = [];
}
