using CodeImpact.Domain.Entities;
using CodeImpact.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace CodeImpact.Infrastructure.Persistence;

public class GitHubPullRequestRepository : IGitHubPullRequestRepository
{
    private readonly CodeImpactDbContext _dbContext;

    public GitHubPullRequestRepository(CodeImpactDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<GitHubPullRequest?> GetByUserRepositoryAndGitHubPullRequestIdAsync(Guid userId, long repositoryId, long gitHubPullRequestId)
    {
        return _dbContext.GitHubPullRequests.FirstOrDefaultAsync(
            pr => pr.UserId == userId && pr.RepositoryId == repositoryId && pr.GitHubPullRequestId == gitHubPullRequestId);
    }

    public async Task<IReadOnlyCollection<GitHubPullRequest>> ListByUserAsync(Guid userId, long? repositoryId, DateTime? from, DateTime? to)
    {
        var query = _dbContext.GitHubPullRequests
            .AsNoTracking()
            .Where(pr => pr.UserId == userId);

        if (repositoryId.HasValue)
        {
            query = query.Where(pr => pr.RepositoryId == repositoryId.Value);
        }

        if (from.HasValue)
        {
            query = query.Where(pr => pr.CreatedAtGitHub >= from.Value);
        }

        if (to.HasValue)
        {
            var exclusiveUpperBound = to.Value.Date.AddDays(1);
            query = query.Where(pr => pr.CreatedAtGitHub < exclusiveUpperBound);
        }

        return await query
            .OrderByDescending(pr => pr.CreatedAtGitHub)
            .ToListAsync();
    }

    public Task<GitHubPullRequest?> GetByIdAsync(Guid userId, Guid pullRequestId)
    {
        return _dbContext.GitHubPullRequests
            .AsNoTracking()
            .FirstOrDefaultAsync(pr => pr.UserId == userId && pr.Id == pullRequestId);
    }

    public async Task AddAsync(GitHubPullRequest pullRequest)
    {
        _dbContext.GitHubPullRequests.Add(pullRequest);
        await _dbContext.SaveChangesAsync();
    }

    public async Task UpdateAsync(GitHubPullRequest pullRequest)
    {
        _dbContext.GitHubPullRequests.Update(pullRequest);
        await _dbContext.SaveChangesAsync();
    }
}