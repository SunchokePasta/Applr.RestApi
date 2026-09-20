using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Applr.RestApi.Entities;

/// <summary>
/// One way of recognising a field on a page. A field has many of these, so a
/// newly discovered wording is an INSERT rather than an edit to a regex
/// string -- which is what makes a bad addition easy to isolate and reverse.
/// </summary>
[Table("profile_field_patterns")]
public sealed class ProfileFieldPattern
{
    [Key]
    [Column("id")]
    public uint Id { get; set; }

    [Column("field_key")]
    public string FieldKey { get; set; } = string.Empty;

    /// <summary>'regex' or 'literal'. Literal covers a label reported verbatim.</summary>
    [Column("match_type")]
    public string MatchType { get; set; } = "regex";

    [Column("pattern")]
    public string Pattern { get; set; } = string.Empty;

    /// <summary>
    /// Empty string means global. Scoped because the same wording can mean
    /// different things on different sites -- "Reference" is a reference
    /// number on one job board and a referee on another. Empty rather than
    /// NULL so the UNIQUE constraint actually catches duplicates: in SQL,
    /// NULL never equals NULL, so a nullable column lets identical global
    /// rows both through.
    /// </summary>
    [Column("domain")]
    public string Domain { get; set; } = string.Empty;

    /// <summary>'builtin', 'llm' or 'user' -- so a bad batch can be found and purged.</summary>
    [Column("source")]
    public string Source { get; set; } = "builtin";

    /// <summary>
    /// 'pending', 'active' or 'rejected'. Only 'active' is used for matching.
    /// Rejected rows are kept deliberately, so the same wrong suggestion can
    /// be recognised rather than proposed again.
    /// </summary>
    [Column("status")]
    public string Status { get; set; } = "active";

    [Column("confidence")]
    public decimal? Confidence { get; set; }

    /// <summary>Ties a suggestion back to the run that produced it in the filler's own logs.</summary>
    [Column("llm_run_id")]
    public string? LlmRunId { get; set; }

    [Column("created_on")]
    public DateTime CreatedOn { get; set; }
}
