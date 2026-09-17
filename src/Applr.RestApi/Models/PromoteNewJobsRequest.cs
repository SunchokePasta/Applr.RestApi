namespace Applr.RestApi.Models;

/// <summary>Body for POST jobs/promote-new -- the specific job ids to flip from 'New' to 'Unreviewed'.</summary>
public sealed class PromoteNewJobsRequest
{
    public List<uint> JobIds { get; set; } = [];
}
