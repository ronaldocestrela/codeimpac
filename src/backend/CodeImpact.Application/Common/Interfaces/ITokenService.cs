using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CodeImpact.Application.Common.Interfaces
{
    public interface ITokenService
    {
        Task<string> CreateAccessTokenAsync(Guid userId, string email, IEnumerable<string> roles);

        Task<string> CreateRefreshTokenAsync();

        Task<bool> ValidateRefreshTokenAsync(string refreshToken);
    }
}
