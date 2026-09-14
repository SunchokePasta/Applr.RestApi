using Applr.RestApi.Data;
using Applr.RestApi.Entities;
using Microsoft.EntityFrameworkCore;

namespace Applr.RestApi.Repositories;

public sealed class JobRepository(ApplrDbContext dbContext) : IJobRepository
{
    public Task<IReadOnlyList<Job>> GetByCompanyNameAsync(string companyName, CancellationToken cancellationToken = default) =>
        QueryAsync(j => j.CompanyName == companyName, cancellationToken);

    public Task<IReadOnlyList<Job>> GetByJobTitleAsync(string jobTitle, CancellationToken cancellationToken = default) =>
        QueryAsync(j => j.JobTitle == jobTitle, cancellationToken);

    public Task<IReadOnlyList<Job>> GetByStatusAsync(string status, CancellationToken cancellationToken = default) =>
        QueryAsync(j => j.Status == status, cancellationToken);

    public Task<IReadOnlyList<Job>> GetByPostedDateAsync(DateOnly postedDate, CancellationToken cancellationToken = default) =>
        QueryAsync(j => j.PostedDate == postedDate, cancellationToken);

    public Task<IReadOnlyList<Job>> GetByCloseDateAsync(DateOnly closeDate, CancellationToken cancellationToken = default) =>
        QueryAsync(j => j.CloseDate == closeDate, cancellationToken);

    public Task<IReadOnlyList<Job>> GetByCvRequiredAsync(bool cvRequired, CancellationToken cancellationToken = default) =>
        QueryAsync(j => j.CvRequired == cvRequired, cancellationToken);

    public Task<IReadOnlyList<Job>> GetByCoverLetterRequiredAsync(bool coverLetterRequired, CancellationToken cancellationToken = default) =>
        QueryAsync(j => j.CoverLetterRequired == coverLetterRequired, cancellationToken);

    public Task<IReadOnlyList<Job>> GetByWrittenAnswersRequiredAsync(bool writtenAnswersRequired, CancellationToken cancellationToken = default) =>
        QueryAsync(j => j.WrittenAnswersRequired == writtenAnswersRequired, cancellationToken);

    public Task<IReadOnlyList<Job>> GetByVisaSponsorshipAsync(bool visaSponsorship, CancellationToken cancellationToken = default) =>
        QueryAsync(j => j.VisaSponsorship == visaSponsorship, cancellationToken);

    public async Task<IReadOnlyList<Job>> QueryAsync(JobQueryFilter filter, CancellationToken cancellationToken = default)
    {
        IQueryable<Job> query = dbContext.Jobs.AsNoTracking();

        if (filter.CompanyName is not null)
        {
            query = query.Where(j => j.CompanyName == filter.CompanyName);
        }

        if (filter.JobTitle is not null)
        {
            query = query.Where(j => j.JobTitle == filter.JobTitle);
        }

        if (filter.Status is not null)
        {
            query = query.Where(j => j.Status == filter.Status);
        }

        if (filter.PostedDate is not null)
        {
            query = query.Where(j => j.PostedDate == filter.PostedDate);
        }

        if (filter.CloseDate is not null)
        {
            query = query.Where(j => j.CloseDate == filter.CloseDate);
        }

        if (filter.CvRequired is not null)
        {
            query = query.Where(j => j.CvRequired == filter.CvRequired);
        }

        if (filter.CoverLetterRequired is not null)
        {
            query = query.Where(j => j.CoverLetterRequired == filter.CoverLetterRequired);
        }

        if (filter.WrittenAnswersRequired is not null)
        {
            query = query.Where(j => j.WrittenAnswersRequired == filter.WrittenAnswersRequired);
        }

        if (filter.VisaSponsorship is not null)
        {
            query = query.Where(j => j.VisaSponsorship == filter.VisaSponsorship);
        }

        return await query.ToListAsync(cancellationToken);
    }

    public Task<Job?> GetByIdAsync(uint id, CancellationToken cancellationToken = default) =>
        dbContext.Jobs.FirstOrDefaultAsync(j => j.Id == id, cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        dbContext.SaveChangesAsync(cancellationToken);

    private async Task<IReadOnlyList<Job>> QueryAsync(
        System.Linq.Expressions.Expression<Func<Job, bool>> predicate,
        CancellationToken cancellationToken) =>
        await dbContext.Jobs.AsNoTracking().Where(predicate).ToListAsync(cancellationToken);
}
