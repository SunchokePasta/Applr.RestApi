using Applr.RestApi.Data;
using Applr.RestApi.Entities;
using Microsoft.EntityFrameworkCore;

namespace Applr.RestApi.Repositories;

public sealed class JobRepository(ApplrDbContext dbContext) : IJobRepository
{
    public async Task<IReadOnlyList<Job>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await dbContext.Jobs.AsNoTracking().ToListAsync(cancellationToken);

    public Task<Job?> GetByIdAsync(uint id, CancellationToken cancellationToken = default) =>
        dbContext.Jobs.AsNoTracking().FirstOrDefaultAsync(j => j.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Job>> GetByStatusAsync(string status, CancellationToken cancellationToken = default) =>
        await dbContext.Jobs.AsNoTracking().Where(j => j.Status == status).ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Job>> GetByCompanyIdAsync(uint companyId, CancellationToken cancellationToken = default) =>
        await dbContext.Jobs.AsNoTracking().Where(j => j.CompanyId == companyId).ToListAsync(cancellationToken);

    public async Task<Job> CreateAsync(Job job, CancellationToken cancellationToken = default)
    {
        dbContext.Jobs.Add(job);
        await dbContext.SaveChangesAsync(cancellationToken);
        return job;
    }

    public async Task<int> UpdateStatusAsync(IReadOnlyCollection<uint> jobIds, string status, CancellationToken cancellationToken = default)
    {
        if (jobIds.Count == 0)
        {
            return 0;
        }

        // Tracked on purpose (no AsNoTracking) -- these need to be
        // mutated and saved.
        var jobs = await dbContext.Jobs
            .Where(j => jobIds.Contains(j.Id))
            .ToListAsync(cancellationToken);

        var now = DateTime.UtcNow;

        foreach (var job in jobs)
        {
            job.Status = status;
            job.LastUpdated = now;
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return jobs.Count;
    }
}
