using CodeImpact.Domain.Entities;

namespace CodeImpact.Domain.Repositories;

public interface IGitHubPullRequestReviewRepository
{
    Task<GitHubPullRequestReview?> GetByGitHubReviewIdAsync(long gitHubReviewId);
    Task<IReadOnlyCollection<GitHubPullRequestReview>> ListByPullRequestAsync(Guid userId, long repositoryId, long gitHubPullRequestId);
    Task AddAsync(GitHubPullRequestReview review);
    Task UpdateAsync(GitHubPullRequestReview review);
}