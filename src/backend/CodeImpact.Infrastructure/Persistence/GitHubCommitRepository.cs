using CodeImpact.Domain.Entities;
using CodeImpact.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace CodeImpact.Infrastructure.Persistence;

public class GitHubCommitRepository : IGitHubCommitRepository
{
    private readonly CodeImpactDbContext _dbContext;

    public GitHubCommitRepository(CodeImpactDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<GitHubCommit?> GetByUserRepositoryAndShaAsync(Guid userId, long repositoryId, string commitSha)
    {
        return _dbContext.GitHubCommits.FirstOrDefaultAsync(
            c => c.UserId == userId && c.RepositoryId == repositoryId && c.CommitSha == commitSha);
    }

    public async Task AddAsync(GitHubCommit commit)
    {
        _dbContext.GitHubCommits.Add(commit);
        await _dbContext.SaveChangesAsync();
    }

    public async Task UpdateAsync(GitHubCommit commit)
    {
        _dbContext.GitHubCommits.Update(commit);
        await _dbContext.SaveChangesAsync();
    }
}