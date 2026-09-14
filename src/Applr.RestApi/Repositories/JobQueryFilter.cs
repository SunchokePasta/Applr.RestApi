namespace Applr.RestApi.Repositories;

public sealed class JobQueryFilter
{
    public string? CompanyName { get; set; }
    public string? JobTitle { get; set; }
    public string? Status { get; set; }
    public DateOnly? PostedDate { get; set; }
    public DateOnly? CloseDate { get; set; }
    public bool? CvRequired { get; set; }
    public bool? CoverLetterRequired { get; set; }
    public bool? WrittenAnswersRequired { get; set; }
    public bool? VisaSponsorship { get; set; }
}
