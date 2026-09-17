using Applr.RestApi.Entities;

namespace Applr.RestApi.Repositories;

/// <summary>
/// Queries/writes against `companies` only.
/// </summary>
public interface ICompanyRepository
{
    Task<Company?> GetByIdAsync(uint id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Exact match on `name` as given -- trim before calling this,
    /// callers (JobSyncService) own that, not the repository.
    /// </summary>
    Task<Company?> GetByNameAsync(string name, CancellationToken cancellationToken = default);

    Task<Company> CreateAsync(Company company, CancellationToken cancellationToken = default);
}
