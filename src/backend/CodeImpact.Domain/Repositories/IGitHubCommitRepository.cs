using CodeImpact.Domain.Entities;

namespace CodeImpact.Domain.Repositories;

public interface IGitHubCommitRepository
{
    Task<GitHubCommit?> GetByUserRepositoryAndShaAsync(Guid userId, long repositoryId, string commitSha);
    Task AddAsync(GitHubCommit commit);
    Task UpdateAsync(GitHubCommit commit);
}