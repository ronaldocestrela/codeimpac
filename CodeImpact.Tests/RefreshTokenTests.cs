using CodeImpact.Domain.Entities;

namespace CodeImpact.Tests;

public class RefreshTokenTests
{
    [Fact]
    public void New_refresh_token_is_active_until_expiration()
    {
        var refreshToken = new RefreshToken(Guid.NewGuid(), "token-value", DateTime.UtcNow.AddDays(2));

        Assert.True(refreshToken.IsActive);
        Assert.Null(refreshToken.RevokedAt);
        Assert.Equal("token-value", refreshToken.Token);
    }

    [Fact]
    public void Revoke_marks_token_inactive()
    {
        var refreshToken = new RefreshToken(Guid.NewGuid(), "token-value", DateTime.UtcNow.AddDays(2));

        refreshToken.Revoke();

        Assert.False(refreshToken.IsActive);
        Assert.NotNull(refreshToken.RevokedAt);
    }

    [Fact]
    public void Expired_token_is_not_active()
    {
        var refreshToken = new RefreshToken(Guid.NewGuid(), "token-value", DateTime.UtcNow.AddMinutes(-1));

        Assert.False(refreshToken.IsActive);
    }
}
