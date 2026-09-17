using Applr.RestApi.Data;
using Applr.RestApi.Entities;
using Microsoft.EntityFrameworkCore;

namespace Applr.RestApi.Repositories;

public sealed class RawJobRepository(ApplrDbContext dbContext) : IRawJobRepository
{
    public async Task<IReadOnlyList<RawJob>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await dbContext.RawJobs.AsNoTracking().ToListAsync(cancellationToken);

    public Task<RawJob?> GetByIdAsync(uint id, CancellationToken cancellationToken = default) =>
        dbContext.RawJobs.AsNoTracking().FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

    public Task<IReadOnlyList<RawJob>> GetByCompanyNameAsync(string companyName, CancellationToken cancellationToken = default) =>
        QueryAsync(r => r.CompanyName == companyName, cancellationToken);

    public Task<IReadOnlyList<RawJob>> GetByJobTitleAsync(string jobTitle, CancellationToken cancellationToken = default) =>
        QueryAsync(r => r.JobTitle == jobTitle, cancellationToken);

    public Task<IReadOnlyList<RawJob>> GetByPostedDateAsync(DateOnly postedDate, CancellationToken cancellationToken = default) =>
        QueryAsync(r => r.PostedDate == postedDate, cancellationToken);

    public Task<IReadOnlyList<RawJob>> GetByCloseDateAsync(DateOnly closeDate, CancellationToken cancellationToken = default) =>
        QueryAsync(r => r.CloseDate == closeDate, cancellationToken);

    public Task<IReadOnlyList<RawJob>> GetByCvRequiredAsync(bool cvRequired, CancellationToken cancellationToken = default) =>
        QueryAsync(r => r.CvRequired == cvRequired, cancellationToken);

    public Task<IReadOnlyList<RawJob>> GetByCoverLetterRequiredAsync(bool coverLetterRequired, CancellationToken cancellationToken = default) =>
        QueryAsync(r => r.CoverLetterRequired == coverLetterRequired, cancellationToken);

    public Task<IReadOnlyList<RawJob>> GetByWrittenAnswersRequiredAsync(bool writtenAnswersRequired, CancellationToken cancellationToken = default) =>
        QueryAsync(r => r.WrittenAnswersRequired == writtenAnswersRequired, cancellationToken);

    public Task<IReadOnlyList<RawJob>> GetByVisaSponsorshipAsync(bool visaSponsorship, CancellationToken cancellationToken = default) =>
        QueryAsync(r => r.VisaSponsorship == visaSponsorship, cancellationToken);

    public async Task<IReadOnlyList<RawJob>> QueryAsync(RawJobQueryFilter filter, CancellationToken cancellationToken = default)
    {
        IQueryable<RawJob> query = dbContext.RawJobs.AsNoTracking();

        if (filter.CompanyName is not null)
        {
            query = query.Where(r => r.CompanyName == filter.CompanyName);
        }

        if (filter.JobTitle is not null)
        {
            query = query.Where(r => r.JobTitle == filter.JobTitle);
        }

        if (filter.PostedDate is not null)
        {
            query = query.Where(r => r.PostedDate == filter.PostedDate);
        }

        if (filter.CloseDate is not null)
        {
            query = query.Where(r => r.CloseDate == filter.CloseDate);
        }

        if (filter.CvRequired is not null)
        {
            query = query.Where(r => r.CvRequired == filter.CvRequired);
        }

        if (filter.CoverLetterRequired is not null)
        {
            query = query.Where(r => r.CoverLetterRequired == filter.CoverLetterRequired);
        }

        if (filter.WrittenAnswersRequired is not null)
        {
            query = query.Where(r => r.WrittenAnswersRequired == filter.WrittenAnswersRequired);
        }

        if (filter.VisaSponsorship is not null)
        {
            query = query.Where(r => r.VisaSponsorship == filter.VisaSponsorship);
        }

        return await query.ToListAsync(cancellationToken);
    }

    private async Task<IReadOnlyList<RawJob>> QueryAsync(
        System.Linq.Expressions.Expression<Func<RawJob, bool>> predicate,
        CancellationToken cancellationToken) =>
        await dbContext.RawJobs.AsNoTracking().Where(predicate).ToListAsync(cancellationToken);
}
