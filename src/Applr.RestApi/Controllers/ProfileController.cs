using Applr.RestApi.Entities;
using Applr.RestApi.Models;
using Applr.RestApi.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Applr.RestApi.Controllers;

/// <summary>
/// The profile ApplrFiller fills forms from: which fields exist, what text
/// identifies them on a page, and what a given user answers.
///
/// Called directly by ApplrFiller rather than through Applr.API, on the same
/// reasoning as Scraper.py's db-sync call -- the filler is a peer producer
/// and consumer of this data, not a desktop client, so routing it through the
/// backend-for-frontend would mean two processes running for a hop that adds
/// nothing.
///
/// No try/catch anywhere in here: ExceptionHandlingMiddleware is registered
/// first in the pipeline and turns a duplicate key into a 409, a missing
/// database into a 503, and anything else into a referenced 500.
/// </summary>
[ApiController]
[Route("profile")]
public sealed class ProfileController(IProfileRepository profileRepository) : ControllerBase
{
    /// <summary>
    /// Every field with its active patterns, in match order. One call is all
    /// ApplrFiller needs to rebuild its matcher.
    /// </summary>
    [HttpGet("fields")]
    public async Task<ActionResult<IReadOnlyList<ProfileFieldResponse>>> GetFields(CancellationToken cancellationToken)
    {
        var fields = await profileRepository.GetFieldsAsync(cancellationToken);
        var patterns = await profileRepository.GetActivePatternsAsync(cancellationToken);

        var byField = patterns
            .GroupBy(p => p.FieldKey)
            .ToDictionary(g => g.Key, g => g.Select(p => p.Pattern).ToList());

        return Ok(fields.Select(f => new ProfileFieldResponse
        {
            FieldKey = f.FieldKey,
            DisplayName = f.DisplayName,
            MatchOrder = f.MatchOrder,
            MirrorsKey = f.MirrorsKey,
            IsSensitive = f.IsSensitive,
            Patterns = byField.TryGetValue(f.FieldKey, out var found) ? found : [],
        }).ToList());
    }

    /// <summary>One user's answers, as a field key to value map.</summary>
    [HttpGet("values/{userId}")]
    public async Task<ActionResult<Dictionary<string, string?>>> GetValues(uint userId, CancellationToken cancellationToken)
    {
        var values = await profileRepository.GetValuesAsync(userId, cancellationToken);
        return Ok(values.ToDictionary(v => v.FieldKey, v => v.Value));
    }

    /// <summary>
    /// Adds a wording for a field that already exists -- the resolver's
    /// 'existing' verdict, where the data is held and only the label was
    /// unfamiliar. Lands as 'pending' by default so it is stored without
    /// affecting matching until confirmed.
    /// </summary>
    [HttpPost("fields/{fieldKey}/patterns")]
    public async Task<ActionResult<ProfileFieldPattern>> AddPattern(
        string fieldKey,
        [FromBody] CreateFieldPatternRequest request,
        CancellationToken cancellationToken)
    {
        if (await profileRepository.GetFieldAsync(fieldKey, cancellationToken) is null)
        {
            return NotFound(new { message = $"No profile field '{fieldKey}'. Create the field first." });
        }

        var created = await profileRepository.AddPatternAsync(
            new ProfileFieldPattern
            {
                FieldKey = fieldKey,
                Pattern = request.Pattern,
                MatchType = request.MatchType,
                Domain = request.Domain,
                Source = request.Source,
                Status = request.Status,
                Confidence = request.Confidence,
                LlmRunId = request.LlmRunId,
            },
            cancellationToken);

        return CreatedAtAction(nameof(GetFields), new { }, created);
    }

    /// <summary>
    /// Creates a field and its first pattern together -- the resolver's 'new'
    /// verdict. MatchOrder is appended past the existing fields unless one is
    /// given, since an arbitrary position can break matching that works.
    /// </summary>
    [HttpPost("fields")]
    public async Task<ActionResult<ProfileField>> CreateField(
        [FromBody] CreateProfileFieldRequest request,
        CancellationToken cancellationToken)
    {
        if (await profileRepository.GetFieldAsync(request.FieldKey, cancellationToken) is not null)
        {
            return Conflict(new { message = $"Profile field '{request.FieldKey}' already exists." });
        }

        var matchOrder = request.MatchOrder
            ?? await profileRepository.GetMaxMatchOrderAsync(cancellationToken) + 10;

        var field = await profileRepository.AddFieldAsync(
            new ProfileField
            {
                FieldKey = request.FieldKey,
                DisplayName = request.DisplayName,
                MatchOrder = matchOrder,
                MirrorsKey = request.MirrorsKey,
                IsSensitive = request.IsSensitive,
            },
            cancellationToken);

        if (!string.IsNullOrWhiteSpace(request.Pattern))
        {
            await profileRepository.AddPatternAsync(
                new ProfileFieldPattern
                {
                    FieldKey = field.FieldKey,
                    Pattern = request.Pattern,
                    MatchType = request.MatchType,
                    Domain = request.Domain,
                    Source = request.Source,
                    Status = request.Status,
                    Confidence = request.Confidence,
                    LlmRunId = request.LlmRunId,
                },
                cancellationToken);
        }

        return CreatedAtAction(nameof(GetFields), new { }, field);
    }

    /// <summary>
    /// Sets one user's answer for one field. Idempotent -- the composite key
    /// means a repeat call updates rather than duplicating.
    /// </summary>
    [HttpPut("values/{userId}/{fieldKey}")]
    public async Task<ActionResult<ProfileValue>> UpsertValue(
        uint userId,
        string fieldKey,
        [FromBody] UpsertProfileValueRequest request,
        CancellationToken cancellationToken)
    {
        if (await profileRepository.GetFieldAsync(fieldKey, cancellationToken) is null)
        {
            return NotFound(new { message = $"No profile field '{fieldKey}'. Create the field first." });
        }

        var saved = await profileRepository.UpsertValueAsync(
            new ProfileValue { UserId = userId, FieldKey = fieldKey, Value = request.Value },
            cancellationToken);

        return Ok(saved);
    }
}
