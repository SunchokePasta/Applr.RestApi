using Applr.RestApi.Entities;
using Applr.RestApi.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Applr.RestApi.Services;

public sealed class JobSyncService(
    IRawJobRepository rawJobRepository,
    IJobRepository jobRepository,
    ICompanyRepository companyRepository,
    ILogger<JobSyncService> logger) : IJobSyncService
{
    public async Task<int> SyncNewJobsAsync(CancellationToken cancellationToken = default)
    {
        // Pull both tables in full and diff in memory -- not a database
        // join. Fine at this app's scale, and keeps every repository
        // single-table.
        var rawJobs = await rawJobRepository.GetAllAsync(cancellationToken);
        var jobs = await jobRepository.GetAllAsync(cancellationToken);

        var trackedRawJobIds = jobs
            .Select(j => j.RawJobId)
            .ToHashSet();

        var missingRawJobs = rawJobs
            .Where(r => !trackedRawJobIds.Contains(r.Id))
            .ToList();

        var insertedCount = 0;

        foreach (var rawJob in missingRawJobs)
        {
            var companyName = rawJob.CompanyName.Trim();

            var company = await companyRepository.GetByNameAsync(companyName, cancellationToken)
                ?? await companyRepository.CreateAsync(
                    new Company
                    {
                        Name = companyName,
                        Url = rawJob.CompanyUrl,
                        CreatedOn = DateTime.UtcNow
                    },
                    cancellationToken);

            try
            {
                await jobRepository.CreateAsync(
                    new Job
                    {
                        RawJobId = rawJob.Id,
                        CompanyId = company.Id,
                        JobTitle = rawJob.JobTitle,
                        JobUrl = rawJob.JobUrl,
                        CompanyName = companyName,
                        Status = "New",
                        PostedDate = rawJob.PostedDate,
                        CloseDate = rawJob.CloseDate,
                        CreatedOn = DateTime.UtcNow,
                        LastUpdated = DateTime.UtcNow
                    },
                    cancellationToken);

                insertedCount++;
            }
            catch (DbUpdateException ex)
            {
                // Most likely the UNIQUE constraint on jobs.raw_job_id --
                // another call already synced this raw job between our
                // diff above and this insert. Not an error, just a
                // no-op for this one row.
                logger.LogInformation(
                    ex,
                    "RawJob {RawJobId} could not be synced -- likely already synced by a concurrent call.",
                    rawJob.Id);
            }
        }

        return insertedCount;
    }
}
