using System;
using System.Threading.Tasks;
using CodeImpact.Domain.Entities;
using CodeImpact.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace CodeImpact.Infrastructure.Persistence
{
    public class GitHubAccountRepository : IGitHubAccountRepository
    {
        private readonly CodeImpactDbContext _dbContext;

        public GitHubAccountRepository(CodeImpactDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public Task<GitHubAccount?> GetByIdAsync(Guid id)
        {
            return _dbContext.GitHubAccounts.FirstOrDefaultAsync(g => g.Id == id);
        }

        public Task<GitHubAccount?> GetByUserIdAsync(Guid userId)
        {
            return _dbContext.GitHubAccounts.FirstOrDefaultAsync(g => g.UserId == userId);
        }

        public Task<GitHubAccount?> GetByGitHubUserIdAsync(long gitHubUserId)
        {
            return _dbContext.GitHubAccounts.FirstOrDefaultAsync(g => g.GitHubUserId == gitHubUserId);
        }

        public async Task AddAsync(GitHubAccount account)
        {
            _dbContext.GitHubAccounts.Add(account);
            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdateAsync(GitHubAccount account)
        {
            _dbContext.GitHubAccounts.Update(account);
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteAsync(GitHubAccount account)
        {
            _dbContext.GitHubAccounts.Remove(account);
            await _dbContext.SaveChangesAsync();
        }
    }
}
