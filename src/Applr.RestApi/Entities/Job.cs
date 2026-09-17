using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Applr.RestApi.Entities;

/// <summary>
/// Maps 1:1 onto the `jobs` table -- a RawJob synced into your tracker.
/// Created only by JobSyncService.SyncNewJobsAsync (status 'New'), then
/// updated to 'Unreviewed' via JobRepository.UpdateStatusAsync once the
/// dashboard has shown it. PostedDate/CloseDate/JobTitle/JobUrl/CompanyName
/// are copied in from the matching RawJob (and Company) at insert time and
/// live independently from then on -- no navigation properties on purpose,
/// JobRepository only ever queries this table directly.
/// </summary>
[Table("jobs")]
public sealed class Job
{
    [Key]
    [Column("id")]
    public uint Id { get; set; }

    // UNIQUE at the DB level -- one Job per RawJob. That constraint is
    // what makes SyncNewJobsAsync safe to call more than once for the
    // same raw job.
    [Column("raw_job_id")]
    public uint RawJobId { get; set; }

    [Column("company_id")]
    public uint CompanyId { get; set; }

    // Denormalized from RawJob/Company at sync time so list reads (jobs
    // GET existing/new) don't need a join back to raw_jobs or companies.
    [Column("job_title")]
    public string JobTitle { get; set; } = string.Empty;

    [Column("job_url")]
    public string JobUrl { get; set; } = string.Empty;

    [Column("company_name")]
    public string CompanyName { get; set; } = string.Empty;

    [Column("status")]
    public string Status { get; set; } = "New";

    [Column("posted_date")]
    public DateOnly? PostedDate { get; set; }

    [Column("close_date")]
    public DateOnly? CloseDate { get; set; }

    [Column("created_on")]
    public DateTime CreatedOn { get; set; }

    [Column("last_updated")]
    public DateTime LastUpdated { get; set; }
}
