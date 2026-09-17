using Applr.RestApi.Entities;
using Applr.RestApi.Models;
using Applr.RestApi.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Applr.RestApi.Controllers;

[ApiController]
[Route("jobs")]
public sealed class JobsController(
    IJobRepository jobRepository,
    ICompanyRepository companyRepository) : ControllerBase
{
    /// <summary>Plain `jobs` read, status == "Unreviewed".</summary>
    [HttpGet("existing")]
    public async Task<ActionResult<IReadOnlyList<Job>>> GetExisting(CancellationToken cancellationToken) =>
        Ok(await jobRepository.GetByStatusAsync("Unreviewed", cancellationToken));

    /// <summary>Plain `jobs` read, status == "New".</summary>
    [HttpGet("new")]
    public async Task<ActionResult<IReadOnlyList<Job>>> GetNew(CancellationToken cancellationToken) =>
        Ok(await jobRepository.GetByStatusAsync("New", cancellationToken));

    /// <summary>
    /// Updates the given job ids from 'New' to 'Unreviewed'. An UPDATE
    /// by explicit id, not an insert -- naturally safe to call more
    /// than once with the same ids.
    /// </summary>
    [HttpPost("promote-new")]
    public async Task<ActionResult> PromoteNew(
        [FromBody] PromoteNewJobsRequest request,
        CancellationToken cancellationToken)
    {
        var updatedCount = await jobRepository.UpdateStatusAsync(request.JobIds, "Unreviewed", cancellationToken);
        return Ok(new { updatedCount });
    }

    [HttpGet("status/{status}")]
    public async Task<ActionResult<IReadOnlyList<Job>>> GetByStatus(string status, CancellationToken cancellationToken) =>
        Ok(await jobRepository.GetByStatusAsync(status, cancellationToken));

    /// <summary>
    /// Resolves the company by name first (companies.name), then looks
    /// up jobs by its id -- two single-table calls, not a join. An
    /// unknown company name is a 200 with an empty list, not a 404.
    /// </summary>
    [HttpGet("company/{name}")]
    public async Task<ActionResult<IReadOnlyList<Job>>> GetByCompany(string name, CancellationToken cancellationToken)
    {
        var company = await companyRepository.GetByNameAsync(name, cancellationToken);

        if (company is null)
        {
            return Ok(Array.Empty<Job>());
        }

        return Ok(await jobRepository.GetByCompanyIdAsync(company.Id, cancellationToken));
    }
}
