using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Applr.RestApi.Entities;

/// <summary>
/// Maps 1:1 onto the `raw_jobs` table -- the same table the Python
/// Trackr scraper (TrackrScraper/Scraper.py) upserts into. Untouched
/// scraper output; nothing here is ever written by this API directly.
/// Whether a given row has been promoted into a tracked <see cref="Job"/>
/// is determined by JobSyncService comparing this table against `jobs`
/// -- there's no flag on this entity itself, and no navigation property
/// pointing at Job: each repository sticks to its own table only.
/// </summary>
[Table("raw_jobs")]
public sealed class RawJob
{
    // `id` is `int unsigned` in MySQL, not signed -- uint matches exactly.
    [Key]
    [Column("id")]
    public uint Id { get; set; }

    [Column("job_url")]
    public string JobUrl { get; set; } = string.Empty;

    [Column("job_title")]
    public string JobTitle { get; set; } = string.Empty;

    [Column("company_name")]
    public string CompanyName { get; set; } = string.Empty;

    [Column("company_url")]
    public string? CompanyUrl { get; set; }

    [Column("posted_date")]
    public DateOnly? PostedDate { get; set; }

    [Column("posted_date_raw")]
    public string? PostedDateRaw { get; set; }

    [Column("close_date")]
    public DateOnly? CloseDate { get; set; }

    [Column("close_date_raw")]
    public string? CloseDateRaw { get; set; }

    [Column("cv_required")]
    public bool CvRequired { get; set; }

    [Column("cover_letter_required")]
    public bool CoverLetterRequired { get; set; }

    [Column("written_answers_required")]
    public bool WrittenAnswersRequired { get; set; }

    [Column("visa_sponsorship")]
    public bool VisaSponsorship { get; set; }

    // Real `json` column in MySQL -- mapped as a proper List<string> via
    // a value conversion configured in ApplrDbContext (see OnModelCreating),
    // not left as a raw string for callers to parse themselves.
    [Column("raw_cells")]
    public List<string>? RawCells { get; set; }

    // Generated column (company_name + job_title + job_url) -- identity
    // of the row, never written to by this API.
    [Column("job_identity_hash")]
    public string JobIdentityHash { get; set; } = string.Empty;

    [Column("created_on")]
    public DateTime CreatedOn { get; set; }

    [Column("last_updated")]
    public DateTime LastUpdated { get; set; }
}
