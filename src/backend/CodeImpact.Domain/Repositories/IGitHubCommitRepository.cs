using CodeImpact.Domain.Entities;

namespace CodeImpact.Domain.Repositories;

public interface IGitHubCommitRepository
{
    Task<GitHubCommit?> GetByUserRepositoryAndShaAsync(Guid userId, long repositoryId, string commitSha);
    Task<IReadOnlyCollection<GitHubCommit>> ListByUserAsync(Guid userId, long? repositoryId, DateTime? from, DateTime? to);
    Task<GitHubCommit?> GetByIdAsync(Guid userId, Guid commitId);
    Task AddAsync(GitHubCommit commit);
    Task UpdateAsync(GitHubCommit commit);
}