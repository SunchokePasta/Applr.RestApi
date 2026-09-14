using Applr.RestApi.Entities;
using Applr.RestApi.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Applr.RestApi.Controllers;

[ApiController]
[Route("jobs")]
public sealed class JobsController(IJobRepository jobRepository) : ControllerBase
{
    [HttpGet("company/{name}")]
    public async Task<ActionResult<IReadOnlyList<Job>>> GetByCompany(string name, CancellationToken cancellationToken) =>
        Ok(await jobRepository.GetByCompanyNameAsync(name, cancellationToken));

    [HttpGet("title/{title}")]
    public async Task<ActionResult<IReadOnlyList<Job>>> GetByTitle(string title, CancellationToken cancellationToken) =>
        Ok(await jobRepository.GetByJobTitleAsync(title, cancellationToken));

    [HttpGet("status/{status}")]
    public async Task<ActionResult<IReadOnlyList<Job>>> GetByStatus(string status, CancellationToken cancellationToken) =>
        Ok(await jobRepository.GetByStatusAsync(status, cancellationToken));

    [HttpGet("posted-date/{date}")]
    public async Task<ActionResult<IReadOnlyList<Job>>> GetByPostedDate(DateOnly date, CancellationToken cancellationToken) =>
        Ok(await jobRepository.GetByPostedDateAsync(date, cancellationToken));

    [HttpGet("close-date/{date}")]
    public async Task<ActionResult<IReadOnlyList<Job>>> GetByCloseDate(DateOnly date, CancellationToken cancellationToken) =>
        Ok(await jobRepository.GetByCloseDateAsync(date, cancellationToken));

    [HttpGet("cv-required/{value}")]
    public async Task<ActionResult<IReadOnlyList<Job>>> GetByCvRequired(bool value, CancellationToken cancellationToken) =>
        Ok(await jobRepository.GetByCvRequiredAsync(value, cancellationToken));

    [HttpGet("cover-letter-required/{value}")]
    public async Task<ActionResult<IReadOnlyList<Job>>> GetByCoverLetterRequired(bool value, CancellationToken cancellationToken) =>
        Ok(await jobRepository.GetByCoverLetterRequiredAsync(value, cancellationToken));

    [HttpGet("written-answers-required/{value}")]
    public async Task<ActionResult<IReadOnlyList<Job>>> GetByWrittenAnswersRequired(bool value, CancellationToken cancellationToken) =>
        Ok(await jobRepository.GetByWrittenAnswersRequiredAsync(value, cancellationToken));

    [HttpGet("visa-sponsorship/{value}")]
    public async Task<ActionResult<IReadOnlyList<Job>>> GetByVisaSponsorship(bool value, CancellationToken cancellationToken) =>
        Ok(await jobRepository.GetByVisaSponsorshipAsync(value, cancellationToken));

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<Job>>> Get(
        [FromQuery] JobQueryFilter filter,
        CancellationToken cancellationToken) =>
        Ok(await jobRepository.QueryAsync(filter, cancellationToken));
}
