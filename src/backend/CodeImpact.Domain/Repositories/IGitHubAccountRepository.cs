using System;
using System.Threading.Tasks;
using CodeImpact.Domain.Entities;

namespace CodeImpact.Domain.Repositories
{
    public interface IGitHubAccountRepository
    {
        Task<GitHubAccount?> GetByIdAsync(Guid id);
        Task<GitHubAccount?> GetByUserIdAsync(Guid userId);
        Task<GitHubAccount?> GetByGitHubUserIdAsync(long gitHubUserId);
        Task AddAsync(GitHubAccount account);
        Task UpdateAsync(GitHubAccount account);
    }
}
