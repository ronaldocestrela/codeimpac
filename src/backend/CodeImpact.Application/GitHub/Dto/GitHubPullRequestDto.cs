namespace CodeImpact.Application.GitHub.Dto
{
    public sealed record GitHubPullRequestDto(long Id, string Title, string State, string Url);
}
