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