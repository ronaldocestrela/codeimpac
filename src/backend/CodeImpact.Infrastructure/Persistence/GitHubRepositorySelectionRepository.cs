using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CodeImpact.Domain.Entities;
using CodeImpact.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace CodeImpact.Infrastructure.Persistence
{
    public class GitHubRepositorySelectionRepository : IGitHubRepositorySelectionRepository
    {
        private readonly CodeImpactDbContext _dbContext;

        public GitHubRepositorySelectionRepository(CodeImpactDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IReadOnlyCollection<GitHubRepositorySelection>> GetByUserIdAsync(Guid userId)
        {
            return await _dbContext.GitHubRepositorySelections
                .Where(x => x.UserId == userId)
                .OrderBy(x => x.FullName)
                .ToListAsync();
        }

        public Task<GitHubRepositorySelection?> GetByUserAndRepositoryIdAsync(Guid userId, long repositoryId)
        {
            return _dbContext.GitHubRepositorySelections
                .FirstOrDefaultAsync(x => x.UserId == userId && x.RepositoryId == repositoryId);
        }

        public async Task ReplaceForUserAsync(Guid userId, Guid gitHubAccountId, IEnumerable<GitHubRepositorySelection> selections, string? ownerLoginScope = null)
        {
            var existing = await _dbContext.GitHubRepositorySelections
                .Where(x => x.UserId == userId)
                .ToListAsync();

            if (!string.IsNullOrWhiteSpace(ownerLoginScope))
            {
                existing = existing
                    .Where(x => string.Equals(x.OwnerLogin, ownerLoginScope, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            _dbContext.GitHubRepositorySelections.RemoveRange(existing);
            _dbContext.GitHubRepositorySelections.AddRange(selections);
            await _dbContext.SaveChangesAsync();
        }
    }
}
