using Applr.RestApi.Entities;

namespace Applr.RestApi.Repositories;

public interface IJobRepository
{
    Task<IReadOnlyList<Job>> GetByCompanyNameAsync(string companyName, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Job>> GetByJobTitleAsync(string jobTitle, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Job>> GetByStatusAsync(string status, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Job>> GetByPostedDateAsync(DateOnly postedDate, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Job>> GetByCloseDateAsync(DateOnly closeDate, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Job>> GetByCvRequiredAsync(bool cvRequired, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Job>> GetByCoverLetterRequiredAsync(bool coverLetterRequired, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Job>> GetByWrittenAnswersRequiredAsync(bool writtenAnswersRequired, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Job>> GetByVisaSponsorshipAsync(bool visaSponsorship, CancellationToken cancellationToken = default);

    /// <summary>
    /// Fallback for combining filters, or covering a column the
    /// dedicated routes above don't. Only the properties set on
    /// <paramref name="filter"/> are applied.
    /// </summary>
    Task<IReadOnlyList<Job>> QueryAsync(JobQueryFilter filter, CancellationToken cancellationToken = default);

    /// <summary>
    /// Tracked fetch (no AsNoTracking) -- unlike every read above, this
    /// is meant to be mutated and saved, so the change tracker needs to
    /// actually be watching it. Nothing calls this yet; it's here so a
    /// future write path (e.g. updating Status) has a way to load a
    /// single row that can be changed and persisted.
    /// </summary>
    Task<Job?> GetByIdAsync(uint id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Persists whatever changes were made to entities fetched via
    /// GetByIdAsync. Kept on the repository rather than exposing the
    /// DbContext itself, so callers still only ever talk to
    /// IJobRepository.
    /// </summary>
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
