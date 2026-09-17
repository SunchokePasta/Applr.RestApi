namespace Applr.RestApi.Repositories;

/// <summary>
/// Replaces the old JobQueryFilter -- same fields, just against
/// RawJob now (no Status; that column no longer exists on raw_jobs).
/// </summary>
public sealed class RawJobQueryFilter
{
    public string? CompanyName { get; set; }
    public string? JobTitle { get; set; }
    public DateOnly? PostedDate { get; set; }
    public DateOnly? CloseDate { get; set; }
    public bool? CvRequired { get; set; }
    public bool? CoverLetterRequired { get; set; }
    public bool? WrittenAnswersRequired { get; set; }
    public bool? VisaSponsorship { get; set; }
}
