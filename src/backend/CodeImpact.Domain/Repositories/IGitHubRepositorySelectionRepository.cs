using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CodeImpact.Domain.Entities;

namespace CodeImpact.Domain.Repositories
{
    public interface IGitHubRepositorySelectionRepository
    {
        Task<IReadOnlyCollection<GitHubRepositorySelection>> GetByUserIdAsync(Guid userId);
        Task<GitHubRepositorySelection?> GetByUserAndRepositoryIdAsync(Guid userId, long repositoryId);
        Task ReplaceForUserAsync(Guid userId, Guid gitHubAccountId, IEnumerable<GitHubRepositorySelection> selections);
    }
}
