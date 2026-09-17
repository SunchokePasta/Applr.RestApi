using Applr.RestApi.Entities;

namespace Applr.RestApi.Repositories;

/// <summary>
/// Queries/writes against `jobs` only -- no RawJob/Company fields
/// joined in here. Callers that want title/company/etc. alongside a
/// Job fetch RawJobs/Companies separately and assemble it themselves.
/// </summary>
public interface IJobRepository
{
    /// <summary>Plain `SELECT * FROM jobs` -- every tracked job, as-is.</summary>
    Task<IReadOnlyList<Job>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<Job?> GetByIdAsync(uint id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Job>> GetByStatusAsync(string status, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Job>> GetByCompanyIdAsync(uint companyId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Relies on the UNIQUE constraint on raw_job_id to make repeated
    /// calls for the same RawJob safe -- callers (JobSyncService) should
    /// expect and handle a DbUpdateException on a duplicate rather than
    /// pre-checking existence themselves every time.
    /// </summary>
    Task<Job> CreateAsync(Job job, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates status (and bumps LastUpdated) for exactly the given job
    /// ids -- an UPDATE, not an insert, so it's naturally idempotent to
    /// call more than once with the same ids. Returns how many rows
    /// were actually matched/updated (ids that don't exist are just
    /// skipped, not an error).
    /// </summary>
    Task<int> UpdateStatusAsync(IReadOnlyCollection<uint> jobIds, string status, CancellationToken cancellationToken = default);
}
