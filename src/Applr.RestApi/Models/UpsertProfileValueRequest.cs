namespace Applr.RestApi.Models;

/// <summary>Body for PUT profile/values/{userId}/{fieldKey}.</summary>
public sealed class UpsertProfileValueRequest
{
    public string? Value { get; set; }
}
