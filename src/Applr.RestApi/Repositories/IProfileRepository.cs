using Applr.RestApi.Entities;

namespace Applr.RestApi.Repositories;

/// <summary>
/// Queries against profile_fields, profile_field_patterns and profile_values.
/// Those three are one unit -- a field is meaningless without its patterns
/// and a value cannot exist without its field -- so they share a repository
/// rather than following the one-per-table shape used for the job tables.
/// </summary>
public interface IProfileRepository
{
    Task<IReadOnlyList<ProfileField>> GetFieldsAsync(CancellationToken cancellationToken = default);

    Task<ProfileField?> GetFieldAsync(string fieldKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Active patterns only, ordered by their field's MatchOrder. Pending and
    /// rejected rows are stored but never matched against.
    /// </summary>
    Task<IReadOnlyList<ProfileFieldPattern>> GetActivePatternsAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ProfileValue>> GetValuesAsync(uint userId, CancellationToken cancellationToken = default);

    /// <summary>Highest MatchOrder in use, so a new field can be appended past it.</summary>
    Task<int> GetMaxMatchOrderAsync(CancellationToken cancellationToken = default);

    Task<ProfileField> AddFieldAsync(ProfileField field, CancellationToken cancellationToken = default);

    Task<ProfileFieldPattern> AddPatternAsync(ProfileFieldPattern pattern, CancellationToken cancellationToken = default);

    Task<ProfileValue> UpsertValueAsync(ProfileValue value, CancellationToken cancellationToken = default);
}
