using System;
using System.Threading.Tasks;
using CodeImpact.Domain.Entities;

namespace CodeImpact.Domain.Repositories
{
    public interface IRefreshTokenRepository
    {
        Task<RefreshToken?> GetByTokenAsync(string token);

        Task AddAsync(RefreshToken refreshToken);

        Task UpdateAsync(RefreshToken refreshToken);
    }
}
