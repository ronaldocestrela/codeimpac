namespace CodeImpact.Application.GitHub.Dto;

public sealed record PagedContributionsDto(
    IReadOnlyCollection<ContributionListItemDto> Items,
    int TotalCount,
    int CommitCount,
    int PullRequestCount,
    int ApprovedPullRequestCount,
    int Page,
    int PageSize,
    int TotalPages)
{
    public bool HasPreviousPage => Page > 1;

    public bool HasNextPage => Page < TotalPages;
}
