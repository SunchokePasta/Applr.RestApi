using System.ComponentModel.DataAnnotations.Schema;

namespace Applr.RestApi.Entities;

/// <summary>
/// What one user answers for one field. Separate from the patterns because
/// the lifetimes differ: patterns are shared and derivable (wipe and relearn
/// them freely), values are personal and irreplaceable. Deleting a user is
/// then one statement against this table and touches nothing else.
///
/// Keyed on (user_id, field_key) so the database enforces one value per user
/// per field rather than application code remembering to.
/// </summary>
[Table("profile_values")]
public sealed class ProfileValue
{
    [Column("user_id")]
    public uint UserId { get; set; }

    [Column("field_key")]
    public string FieldKey { get; set; } = string.Empty;

    [Column("value")]
    public string? Value { get; set; }

    [Column("updated_on")]
    public DateTime UpdatedOn { get; set; }
}
