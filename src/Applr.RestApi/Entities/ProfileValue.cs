using System.ComponentModel.DataAnnotations.Schema;

namespace Applr.RestApi.Entities;

/// <summary>
/// What one user answers for one field. Separate from the patterns because
/// the lifetimes differ: patterns are shared and derivable (wipe and relearn
/// them freely), values are personal and irreplaceable. Deleting a user is
/// then one statement against this table and touches nothing else.
///
/// A field can hold several answers for one user, ranked: 1 is the preferred
/// one and the rest are fallbacks the filler tries when a page's options do
/// not include the first ("United Kingdom", then "UK", then "England").
/// Keyed on (user_id, field_key, value_rank) so the database enforces one
/// answer per rank rather than application code remembering to.
/// </summary>
[Table("profile_values")]
public sealed class ProfileValue
{
    [Column("user_id")]
    public uint UserId { get; set; }

    [Column("field_key")]
    public string FieldKey { get; set; } = string.Empty;

    /// <summary>1 is the preferred answer; higher numbers are tried later.</summary>
    [Column("value_rank")]
    public byte ValueRank { get; set; } = 1;

    [Column("value")]
    public string? Value { get; set; }

    [Column("created_on")]
    public DateTime CreatedOn { get; set; }

    [Column("updated_on")]
    public DateTime UpdatedOn { get; set; }
}
