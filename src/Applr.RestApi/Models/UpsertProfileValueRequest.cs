namespace Applr.RestApi.Models;

/// <summary>Body for PUT profile/values/{userId}/{fieldKey}.</summary>
public sealed class UpsertProfileValueRequest
{
    public string? Value { get; set; }

    /// <summary>
    /// Which answer this is: 1 (the default) is the preferred one, 2 and up are
    /// fallbacks tried in order. 1 to 255.
    /// </summary>
    public int Rank { get; set; } = 1;
}
