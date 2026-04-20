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

    public async Task<IReadOnlyCollection<GitHubCommit>> ListByUserAsync(Guid userId, long? repositoryId, string? organizationLogin, DateTime? from, DateTime? to)
    {
        var query = _dbContext.GitHubCommits
            .AsNoTracking()
            .Where(c => c.UserId == userId);

        if (repositoryId.HasValue)
        {
            query = query.Where(c => c.RepositoryId == repositoryId.Value);
        }

        if (!string.IsNullOrWhiteSpace(organizationLogin))
        {
            query = query.Where(c => EF.Functions.Like(c.RepositoryFullName, organizationLogin + "/%"));
        }

        if (from.HasValue)
        {
            query = query.Where(c => c.CommittedAt >= from.Value);
        }

        if (to.HasValue)
        {
            var exclusiveUpperBound = to.Value.Date.AddDays(1);
            query = query.Where(c => c.CommittedAt < exclusiveUpperBound);
        }

        return await query
            .OrderByDescending(c => c.CommittedAt)
            .ToListAsync();
    }

    public Task<GitHubCommit?> GetByIdAsync(Guid userId, Guid commitId)
    {
        return _dbContext.GitHubCommits
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.UserId == userId && c.Id == commitId);
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