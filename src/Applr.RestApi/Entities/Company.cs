using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Applr.RestApi.Entities;

/// <summary>
/// Deduped company info, resolved/created during the promote step (see
/// JobSyncService.PromoteNewJobsAsync) from a RawJob's own CompanyName --
/// never written to directly by anything else. `name` has a UNIQUE
/// constraint at the DB level (see the migration SQL); MySQL's default
/// collation makes that comparison case-insensitive, but it won't catch
/// stray whitespace, so matching logic still trims before comparing.
/// </summary>
[Table("companies")]
public sealed class Company
{
    [Key]
    [Column("id")]
    public uint Id { get; set; }

    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Column("url")]
    public string? Url { get; set; }

    [Column("created_on")]
    public DateTime CreatedOn { get; set; }
}
