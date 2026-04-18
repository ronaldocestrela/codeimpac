using System.IdentityModel.Tokens.Jwt;
using CodeImpact.Infrastructure.Services;
using CodeImpact.Infrastructure.Settings;
using Microsoft.Extensions.Options;

namespace CodeImpact.Tests;

public class TokenServiceTests
{
    private readonly TokenService _tokenService;

    public TokenServiceTests()
    {
        var jwtSettings = new JwtSettings
        {
            Issuer = "https://localhost:7243",
            Audience = "https://localhost:7243",
            Secret = "ASecretKeyOfAtLeast32Characters!",
            AccessTokenExpirationMinutes = 30,
            RefreshTokenExpirationDays = 30
        };

        var options = Options.Create(jwtSettings);
        _tokenService = new TokenService(options);
    }

    [Fact]
    public async Task CreateAccessTokenAsync_ReturnsValidJwt()
    {
        var token = await _tokenService.CreateAccessTokenAsync(Guid.NewGuid(), "user@example.com", Array.Empty<string>());

        Assert.False(string.IsNullOrWhiteSpace(token));

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        Assert.Equal("https://localhost:7243", jwt.Issuer);
        Assert.Equal("https://localhost:7243", jwt.Audiences.Single());
        Assert.Contains(jwt.Claims, c => c.Type == JwtRegisteredClaimNames.Email && c.Value == "user@example.com");
        Assert.Contains(jwt.Claims, c => c.Type == JwtRegisteredClaimNames.Sub);
    }

    [Fact]
    public async Task CreateRefreshTokenAsync_ReturnsNonEmptyString()
    {
        var refreshToken = await _tokenService.CreateRefreshTokenAsync();

        Assert.False(string.IsNullOrWhiteSpace(refreshToken));
        Assert.True(refreshToken.Length >= 86); // Base64 64 bytes produces at least 86 chars
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task ValidateRefreshTokenAsync_InvalidToken_ReturnsFalse(string? token)
    {
        var isValid = await _tokenService.ValidateRefreshTokenAsync(token!);

        Assert.False(isValid);
    }
}
