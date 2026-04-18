using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using CodeImpact.Application.Common.Interfaces;
using CodeImpact.Infrastructure.Settings;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace CodeImpact.Infrastructure.Services
{
    public class TokenService : ITokenService
    {
        private readonly JwtSettings _jwtSettings;

        public TokenService(IOptions<JwtSettings> jwtOptions)
        {
            _jwtSettings = jwtOptions.Value;
        }

        public Task<string> CreateAccessTokenAsync(Guid userId, string email, IEnumerable<string> roles)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes);

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: expires,
                signingCredentials: credentials);

            return Task.FromResult(new JwtSecurityTokenHandler().WriteToken(token));
        }

        public Task<string> CreateRefreshTokenAsync()
        {
            var buffer = RandomNumberGenerator.GetBytes(64);
            var refreshToken = Convert.ToBase64String(buffer);
            return Task.FromResult(refreshToken);
        }

        public Task<bool> ValidateRefreshTokenAsync(string refreshToken)
        {
            return Task.FromResult(!string.IsNullOrWhiteSpace(refreshToken));
        }
    }
}
