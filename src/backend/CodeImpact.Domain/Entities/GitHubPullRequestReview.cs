using CodeImpact.Domain.Common;

namespace CodeImpact.Domain.Entities;

public class GitHubPullRequestReview : BaseEntity
{
    public Guid UserId { get; private set; }
    public Guid GitHubAccountId { get; private set; }
    public long RepositoryId { get; private set; }
    public string RepositoryFullName { get; private set; } = string.Empty;
    public long GitHubPullRequestId { get; private set; }
    public long GitHubReviewId { get; private set; }
    public string ReviewerLogin { get; private set; } = string.Empty;
    public string State { get; private set; } = string.Empty;
    public DateTime SubmittedAt { get; private set; }
    public string Url { get; private set; } = string.Empty;

    private GitHubPullRequestReview() { }

    public GitHubPullRequestReview(
        Guid userId,
        Guid gitHubAccountId,
        long repositoryId,
        string repositoryFullName,
        long gitHubPullRequestId,
        long gitHubReviewId,
        string reviewerLogin,
        string state,
        DateTime submittedAt,
        string url)
    {
        UserId = userId;
        GitHubAccountId = gitHubAccountId;
        RepositoryId = repositoryId;
        RepositoryFullName = repositoryFullName;
        GitHubPullRequestId = gitHubPullRequestId;
        GitHubReviewId = gitHubReviewId;
        ReviewerLogin = reviewerLogin;
        State = state;
        SubmittedAt = submittedAt;
        Url = url;
    }

    public void UpdateFromSync(string reviewerLogin, string state, DateTime submittedAt, string url)
    {
        ReviewerLogin = reviewerLogin;
        State = state;
        SubmittedAt = submittedAt;
        Url = url;
        SetUpdated();
    }
}