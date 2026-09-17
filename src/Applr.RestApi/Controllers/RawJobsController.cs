using Applr.RestApi.Entities;
using Applr.RestApi.Repositories;
using Applr.RestApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace Applr.RestApi.Controllers;

/// <summary>
/// The old JobsController's per-field filters live here since every one
/// of those fields (company, title, dates, the booleans) belongs to
/// RawJob now, not Job. Also owns db-sync: it's driven by reading
/// raw_jobs, even though it writes into jobs as a side effect.
/// </summary>
[ApiController]
[Route("raw-jobs")]
public sealed class RawJobsController(
    IRawJobRepository rawJobRepository,
    IJobSyncService jobSyncService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<RawJob>>> GetAll(CancellationToken cancellationToken) =>
        Ok(await rawJobRepository.GetAllAsync(cancellationToken));

    [HttpGet("company/{name}")]
    public async Task<ActionResult<IReadOnlyList<RawJob>>> GetByCompany(string name, CancellationToken cancellationToken) =>
        Ok(await rawJobRepository.GetByCompanyNameAsync(name, cancellationToken));

    [HttpGet("title/{title}")]
    public async Task<ActionResult<IReadOnlyList<RawJob>>> GetByTitle(string title, CancellationToken cancellationToken) =>
        Ok(await rawJobRepository.GetByJobTitleAsync(title, cancellationToken));

    [HttpGet("posted-date/{date}")]
    public async Task<ActionResult<IReadOnlyList<RawJob>>> GetByPostedDate(DateOnly date, CancellationToken cancellationToken) =>
        Ok(await rawJobRepository.GetByPostedDateAsync(date, cancellationToken));

    [HttpGet("close-date/{date}")]
    public async Task<ActionResult<IReadOnlyList<RawJob>>> GetByCloseDate(DateOnly date, CancellationToken cancellationToken) =>
        Ok(await rawJobRepository.GetByCloseDateAsync(date, cancellationToken));

    [HttpGet("cv-required/{value}")]
    public async Task<ActionResult<IReadOnlyList<RawJob>>> GetByCvRequired(bool value, CancellationToken cancellationToken) =>
        Ok(await rawJobRepository.GetByCvRequiredAsync(value, cancellationToken));

    [HttpGet("cover-letter-required/{value}")]
    public async Task<ActionResult<IReadOnlyList<RawJob>>> GetByCoverLetterRequired(bool value, CancellationToken cancellationToken) =>
        Ok(await rawJobRepository.GetByCoverLetterRequiredAsync(value, cancellationToken));

    [HttpGet("written-answers-required/{value}")]
    public async Task<ActionResult<IReadOnlyList<RawJob>>> GetByWrittenAnswersRequired(bool value, CancellationToken cancellationToken) =>
        Ok(await rawJobRepository.GetByWrittenAnswersRequiredAsync(value, cancellationToken));

    [HttpGet("visa-sponsorship/{value}")]
    public async Task<ActionResult<IReadOnlyList<RawJob>>> GetByVisaSponsorship(bool value, CancellationToken cancellationToken) =>
        Ok(await rawJobRepository.GetByVisaSponsorshipAsync(value, cancellationToken));

    [HttpGet("query")]
    public async Task<ActionResult<IReadOnlyList<RawJob>>> Query(
        [FromQuery] RawJobQueryFilter filter,
        CancellationToken cancellationToken) =>
        Ok(await rawJobRepository.QueryAsync(filter, cancellationToken));

    /// <summary>
    /// Diffs raw_jobs against jobs and inserts a Job (status 'New') for
    /// every raw job still missing one. Safe to call more than once --
    /// relies on the UNIQUE constraint on jobs.raw_job_id, not a
    /// pre-check, to avoid duplicates.
    /// </summary>
    [HttpPost("db-sync")]
    public async Task<ActionResult> DbSync(CancellationToken cancellationToken)
    {
        var insertedCount = await jobSyncService.SyncNewJobsAsync(cancellationToken);
        return Ok(new { insertedCount });
    }
}
