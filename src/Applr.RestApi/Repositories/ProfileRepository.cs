using Applr.RestApi.Data;
using Applr.RestApi.Entities;
using Microsoft.EntityFrameworkCore;

namespace Applr.RestApi.Repositories;

public sealed class ProfileRepository(ApplrDbContext dbContext) : IProfileRepository
{
    public async Task<IReadOnlyList<ProfileField>> GetFieldsAsync(CancellationToken cancellationToken = default) =>
        await dbContext.ProfileFields
            .AsNoTracking()
            .OrderBy(f => f.MatchOrder)
            .ToListAsync(cancellationToken);

    public Task<ProfileField?> GetFieldAsync(string fieldKey, CancellationToken cancellationToken = default) =>
        dbContext.ProfileFields
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.FieldKey == fieldKey, cancellationToken);

    public async Task<IReadOnlyList<ProfileFieldPattern>> GetActivePatternsAsync(CancellationToken cancellationToken = default) =>
        await dbContext.ProfileFieldPatterns
            .AsNoTracking()
            .Where(p => p.Status == "active")
            .OrderBy(p => p.Id)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<ProfileValue>> GetValuesAsync(uint userId, CancellationToken cancellationToken = default) =>
        await dbContext.ProfileValues
            .AsNoTracking()
            .Where(v => v.UserId == userId)
            .OrderBy(v => v.FieldKey)
            .ThenBy(v => v.ValueRank)
            .ToListAsync(cancellationToken);

    public async Task<int> GetMaxMatchOrderAsync(CancellationToken cancellationToken = default) =>
        await dbContext.ProfileFields.AnyAsync(cancellationToken)
            ? await dbContext.ProfileFields.MaxAsync(f => f.MatchOrder, cancellationToken)
            : 0;

    public async Task<ProfileField> AddFieldAsync(ProfileField field, CancellationToken cancellationToken = default)
    {
        dbContext.ProfileFields.Add(field);
        await dbContext.SaveChangesAsync(cancellationToken);
        return field;
    }

    public async Task<ProfileFieldPattern> AddPatternAsync(ProfileFieldPattern pattern, CancellationToken cancellationToken = default)
    {
        dbContext.ProfileFieldPatterns.Add(pattern);
        await dbContext.SaveChangesAsync(cancellationToken);
        return pattern;
    }

    /// <summary>
    /// Read-then-insert-or-update rather than a raw upsert: the primary key is
    /// composite (user, field, rank) and supplied by the caller, so EF cannot
    /// tell a new row from an existing one on its own. Only the row at the
    /// given rank is touched; the field's other ranks are left as they are. A concurrent insert between the two steps
    /// surfaces as a DbUpdateException, which the middleware already maps to a
    /// 409 -- correct behaviour here rather than something to pre-empt.
    /// </summary>
    public async Task<ProfileValue> UpsertValueAsync(ProfileValue value, CancellationToken cancellationToken = default)
    {
        var existing = await dbContext.ProfileValues
            .FirstOrDefaultAsync(
                v => v.UserId == value.UserId
                     && v.FieldKey == value.FieldKey
                     && v.ValueRank == value.ValueRank,
                cancellationToken);

        if (existing is null)
        {
            dbContext.ProfileValues.Add(value);
            await dbContext.SaveChangesAsync(cancellationToken);
            return value;
        }

        existing.Value = value.Value;
        await dbContext.SaveChangesAsync(cancellationToken);
        return existing;
    }
}
