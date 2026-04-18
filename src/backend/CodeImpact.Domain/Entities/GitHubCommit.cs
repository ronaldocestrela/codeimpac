using CodeImpact.Domain.Common;

namespace CodeImpact.Domain.Entities;

public class GitHubCommit : BaseEntity
{
    public Guid UserId { get; private set; }
    public Guid GitHubAccountId { get; private set; }
    public long RepositoryId { get; private set; }
    public string RepositoryFullName { get; private set; } = string.Empty;
    public string CommitSha { get; private set; } = string.Empty;
    public string Message { get; private set; } = string.Empty;
    public string AuthorName { get; private set; } = string.Empty;
    public string AuthorEmail { get; private set; } = string.Empty;
    public DateTime CommittedAt { get; private set; }
    public string Url { get; private set; } = string.Empty;

    private GitHubCommit() { }

    public GitHubCommit(
        Guid userId,
        Guid gitHubAccountId,
        long repositoryId,
        string repositoryFullName,
        string commitSha,
        string message,
        string authorName,
        string authorEmail,
        DateTime committedAt,
        string url)
    {
        UserId = userId;
        GitHubAccountId = gitHubAccountId;
        RepositoryId = repositoryId;
        RepositoryFullName = repositoryFullName;
        CommitSha = commitSha;
        Message = message;
        AuthorName = authorName;
        AuthorEmail = authorEmail;
        CommittedAt = committedAt;
        Url = url;
    }

    public void UpdateFromSync(string message, string authorName, string authorEmail, DateTime committedAt, string url)
    {
        Message = message;
        AuthorName = authorName;
        AuthorEmail = authorEmail;
        CommittedAt = committedAt;
        Url = url;
        SetUpdated();
    }
}