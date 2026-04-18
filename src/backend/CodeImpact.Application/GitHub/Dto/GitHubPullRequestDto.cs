namespace CodeImpact.Application.GitHub.Dto
{
    public sealed record GitHubPullRequestDto(
        long Id,
        int Number,
        string Title,
        string State,
        string AuthorLogin,
        DateTime CreatedAt,
        DateTime? ClosedAt,
        DateTime? MergedAt,
        string Url);
}
