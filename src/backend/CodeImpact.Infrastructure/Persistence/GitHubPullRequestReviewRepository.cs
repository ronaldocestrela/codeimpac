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