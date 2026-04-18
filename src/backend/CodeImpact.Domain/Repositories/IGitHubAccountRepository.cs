using System;
using System.Threading.Tasks;
using CodeImpact.Domain.Entities;

namespace CodeImpact.Domain.Repositories
{
    public interface IGitHubAccountRepository
    {
        Task<GitHubAccount?> GetByIdAsync(Guid id);

        Task AddAsync(GitHubAccount account);
    }
}
