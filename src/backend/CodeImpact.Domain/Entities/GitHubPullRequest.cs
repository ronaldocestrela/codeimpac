using CodeImpact.Domain.Common;

namespace CodeImpact.Domain.Entities;

public class GitHubPullRequest : BaseEntity
{
    public Guid UserId { get; private set; }
    public Guid GitHubAccountId { get; private set; }
    public long RepositoryId { get; private set; }
    public string RepositoryFullName { get; private set; } = string.Empty;
    public long GitHubPullRequestId { get; private set; }
    public int Number { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string State { get; private set; } = string.Empty;
    public string AuthorLogin { get; private set; } = string.Empty;
    public bool IsApproved { get; private set; }
    public DateTime CreatedAtGitHub { get; private set; }
    public DateTime? ClosedAtGitHub { get; private set; }
    public DateTime? MergedAtGitHub { get; private set; }
    public string Url { get; private set; } = string.Empty;

    private GitHubPullRequest() { }

    public GitHubPullRequest(
        Guid userId,
        Guid gitHubAccountId,
        long repositoryId,
        string repositoryFullName,
        long gitHubPullRequestId,
        int number,
        string title,
        string state,
        string authorLogin,
        bool isApproved,
        DateTime createdAtGitHub,
        DateTime? closedAtGitHub,
        DateTime? mergedAtGitHub,
        string url)
    {
        UserId = userId;
        GitHubAccountId = gitHubAccountId;
        RepositoryId = repositoryId;
        RepositoryFullName = repositoryFullName;
        GitHubPullRequestId = gitHubPullRequestId;
        Number = number;
        Title = title;
        State = state;
        AuthorLogin = authorLogin;
        IsApproved = isApproved;
        CreatedAtGitHub = createdAtGitHub;
        ClosedAtGitHub = closedAtGitHub;
        MergedAtGitHub = mergedAtGitHub;
        Url = url;
    }

    public void UpdateFromSync(
        string title,
        string state,
        string authorLogin,
        bool isApproved,
        DateTime createdAtGitHub,
        DateTime? closedAtGitHub,
        DateTime? mergedAtGitHub,
        string url)
    {
        Title = title;
        State = state;
        AuthorLogin = authorLogin;
        IsApproved = isApproved;
        CreatedAtGitHub = createdAtGitHub;
        ClosedAtGitHub = closedAtGitHub;
        MergedAtGitHub = mergedAtGitHub;
        Url = url;
        SetUpdated();
    }
}