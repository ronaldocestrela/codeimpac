using System;
using CodeImpact.Domain.Common;

namespace CodeImpact.Domain.Entities
{
    public class GitHubRepositorySelection : BaseEntity
    {
        public Guid UserId { get; private set; }
        public Guid GitHubAccountId { get; private set; }
        public long RepositoryId { get; private set; }
        public string Name { get; private set; } = string.Empty;
        public string FullName { get; private set; } = string.Empty;
        public bool Private { get; private set; }
        public string OwnerLogin { get; private set; } = string.Empty;
        public string OwnerType { get; private set; } = string.Empty;
        public DateTime SelectedAt { get; private set; }

        private GitHubRepositorySelection() { }

        public GitHubRepositorySelection(
            Guid userId,
            Guid gitHubAccountId,
            long repositoryId,
            string name,
            string fullName,
            bool @private,
            string ownerLogin,
            string ownerType)
        {
            UserId = userId;
            GitHubAccountId = gitHubAccountId;
            RepositoryId = repositoryId;
            Name = name;
            FullName = fullName;
            Private = @private;
            OwnerLogin = ownerLogin;
            OwnerType = ownerType;
            SelectedAt = DateTime.UtcNow;
        }
    }
}
