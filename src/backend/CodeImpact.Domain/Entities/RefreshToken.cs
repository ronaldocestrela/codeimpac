using System;
using CodeImpact.Domain.Common;

namespace CodeImpact.Domain.Entities
{
    public class RefreshToken : BaseEntity
    {
        public Guid UserId { get; private set; }

        public string Token { get; private set; } = string.Empty;

        public DateTime ExpiresAt { get; private set; }

        public DateTime CreatedAtUtc { get; private set; }

        public DateTime? RevokedAt { get; private set; }

        private RefreshToken() { }

        public RefreshToken(Guid userId, string token, DateTime expiresAt)
        {
            UserId = userId;
            Token = token;
            ExpiresAt = expiresAt;
            CreatedAtUtc = DateTime.UtcNow;
        }

        public void Revoke()
        {
            RevokedAt = DateTime.UtcNow;
            SetUpdated();
        }

        public bool IsActive => RevokedAt == null && DateTime.UtcNow < ExpiresAt;
    }
}
