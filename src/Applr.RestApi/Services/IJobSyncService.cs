namespace Applr.RestApi.Services;

/// <summary>
/// The one piece allowed to know about more than one table -- pulls
/// RawJobs and Jobs each via their own repository and diffs them in
/// plain C#/LINQ, rather than a database-side join.
/// </summary>
public interface IJobSyncService
{
    /// <summary>
    /// Diffs raw_jobs against jobs and inserts a Job (status 'New',
    /// PostedDate/CloseDate copied from the RawJob) for every RawJob
    /// still missing one. Returns how many were actually inserted.
    /// </summary>
    Task<int> SyncNewJobsAsync(CancellationToken cancellationToken = default);
}
