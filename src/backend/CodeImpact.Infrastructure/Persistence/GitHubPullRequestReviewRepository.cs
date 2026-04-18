using CodeImpact.Domain.Entities;
using CodeImpact.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace CodeImpact.Infrastructure.Persistence;

public class GitHubPullRequestReviewRepository : IGitHubPullRequestReviewRepository
{
    private readonly CodeImpactDbContext _dbContext;

    public GitHubPullRequestReviewRepository(CodeImpactDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<GitHubPullRequestReview?> GetByGitHubReviewIdAsync(long gitHubReviewId)
    {
        return _dbContext.GitHubPullRequestReviews.FirstOrDefaultAsync(r => r.GitHubReviewId == gitHubReviewId);
    }

    public async Task<IReadOnlyCollection<GitHubPullRequestReview>> ListByPullRequestAsync(Guid userId, long repositoryId, long gitHubPullRequestId)
    {
        return await _dbContext.GitHubPullRequestReviews
            .AsNoTracking()
            .Where(r => r.UserId == userId && r.RepositoryId == repositoryId && r.GitHubPullRequestId == gitHubPullRequestId)
            .OrderByDescending(r => r.SubmittedAt)
            .ToListAsync();
    }

    public async Task AddAsync(GitHubPullRequestReview review)
    {
        _dbContext.GitHubPullRequestReviews.Add(review);
        await _dbContext.SaveChangesAsync();
    }

    public async Task UpdateAsync(GitHubPullRequestReview review)
    {
        _dbContext.GitHubPullRequestReviews.Update(review);
        await _dbContext.SaveChangesAsync();
    }
}