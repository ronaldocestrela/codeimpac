using CodeImpact.Domain.Entities;

namespace CodeImpact.Domain.Repositories;

public interface IGitHubPullRequestRepository
{
    Task<GitHubPullRequest?> GetByUserRepositoryAndGitHubPullRequestIdAsync(Guid userId, long repositoryId, long gitHubPullRequestId);
    Task<IReadOnlyCollection<GitHubPullRequest>> ListByUserAsync(Guid userId, long? repositoryId, string? organizationLogin, DateTime? from, DateTime? to);
    Task<GitHubPullRequest?> GetByIdAsync(Guid userId, Guid pullRequestId);
    Task AddAsync(GitHubPullRequest pullRequest);
    Task UpdateAsync(GitHubPullRequest pullRequest);
}