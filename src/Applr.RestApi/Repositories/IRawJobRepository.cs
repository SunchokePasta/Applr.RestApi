using Applr.RestApi.Entities;

namespace Applr.RestApi.Repositories;

/// <summary>
/// Queries against `raw_jobs` only -- nothing in here ever looks at
/// `jobs` or `companies`.
/// </summary>
public interface IRawJobRepository
{
    Task<IReadOnlyList<RawJob>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<RawJob?> GetByIdAsync(uint id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<RawJob>> GetByCompanyNameAsync(string companyName, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<RawJob>> GetByJobTitleAsync(string jobTitle, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<RawJob>> GetByPostedDateAsync(DateOnly postedDate, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<RawJob>> GetByCloseDateAsync(DateOnly closeDate, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<RawJob>> GetByCvRequiredAsync(bool cvRequired, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<RawJob>> GetByCoverLetterRequiredAsync(bool coverLetterRequired, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<RawJob>> GetByWrittenAnswersRequiredAsync(bool writtenAnswersRequired, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<RawJob>> GetByVisaSponsorshipAsync(bool visaSponsorship, CancellationToken cancellationToken = default);

    /// <summary>
    /// Fallback for combining filters, or covering a column the
    /// dedicated routes above don't. Only the properties set on
    /// <paramref name="filter"/> are applied.
    /// </summary>
    Task<IReadOnlyList<RawJob>> QueryAsync(RawJobQueryFilter filter, CancellationToken cancellationToken = default);
}
