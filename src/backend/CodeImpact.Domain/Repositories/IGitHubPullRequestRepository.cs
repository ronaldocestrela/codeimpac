using CodeImpact.Domain.Entities;

namespace CodeImpact.Domain.Repositories;

public interface IGitHubPullRequestRepository
{
    Task<GitHubPullRequest?> GetByUserRepositoryAndGitHubPullRequestIdAsync(Guid userId, long repositoryId, long gitHubPullRequestId);
    Task AddAsync(GitHubPullRequest pullRequest);
    Task UpdateAsync(GitHubPullRequest pullRequest);
}