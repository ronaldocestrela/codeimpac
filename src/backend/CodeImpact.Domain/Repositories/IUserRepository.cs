using System;
using System.Threading.Tasks;
using CodeImpact.Domain.Entities;

namespace CodeImpact.Domain.Repositories
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(Guid id);

        Task<User?> GetByEmailAsync(string email);

        Task AddAsync(User user);
    }
}
