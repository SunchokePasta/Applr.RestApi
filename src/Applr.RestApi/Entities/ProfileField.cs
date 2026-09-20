using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Applr.RestApi.Entities;

/// <summary>
/// One field an application form might ask for. Shared across every user --
/// personal answers live in ProfileValue, the text that identifies the field
/// on a page lives in ProfileFieldPattern.
///
/// MatchOrder is load-bearing, not decoration: patterns are applied in that
/// order and each input is claimed once, so 'confirm_email' has to be tried
/// before 'email' or the looser pattern takes the confirmation field. It is
/// UNIQUE so two fields can never contend for the same slot. Seeded in tens
/// to leave room to insert between.
/// </summary>
[Table("profile_fields")]
public sealed class ProfileField
{
    [Key]
    [Column("field_key")]
    public string FieldKey { get; set; } = string.Empty;

    [Column("display_name")]
    public string DisplayName { get; set; } = string.Empty;

    [Column("match_order")]
    public int MatchOrder { get; set; }

    /// <summary>
    /// The field this one copies its value from -- 'confirm_email' mirrors
    /// 'email'. A confirmation field is by definition the same value as the
    /// field it confirms, so it is derived rather than stored twice.
    /// </summary>
    [Column("mirrors_key")]
    public string? MirrorsKey { get; set; }

    /// <summary>
    /// Marks an attestation the user is making (right to work, convictions,
    /// "I confirm the above is true") rather than data we hold. Those are
    /// never auto-filled however confident a match is.
    /// </summary>
    [Column("is_sensitive")]
    public bool IsSensitive { get; set; }

    [Column("created_on")]
    public DateTime CreatedOn { get; set; }
}
