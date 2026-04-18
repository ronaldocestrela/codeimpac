using System;
using CodeImpact.Domain.Common;

namespace CodeImpact.Domain.Entities
{
    public class GitHubAccount : BaseEntity
    {
        public Guid UserId { get; private set; }

        public string GitHubUsername { get; private set; } = string.Empty;

        public long GitHubUserId { get; private set; }

        public string EncryptedAccessToken { get; private set; } = string.Empty;

        public DateTime LinkedAt { get; private set; }

        private GitHubAccount() { }

        public GitHubAccount(Guid userId, string gitHubUsername, long gitHubUserId, string encryptedAccessToken)
        {
            UserId = userId;
            GitHubUsername = gitHubUsername;
            GitHubUserId = gitHubUserId;
            EncryptedAccessToken = encryptedAccessToken;
            LinkedAt = DateTime.UtcNow;
        }

        public void UpdateAccessToken(string encryptedAccessToken)
        {
            EncryptedAccessToken = encryptedAccessToken;
            SetUpdated();
        }
    }
}
