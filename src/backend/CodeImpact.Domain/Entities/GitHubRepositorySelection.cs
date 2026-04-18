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
        public DateTime SelectedAt { get; private set; }

        private GitHubRepositorySelection() { }

        public GitHubRepositorySelection(Guid userId, Guid gitHubAccountId, long repositoryId, string name, string fullName, bool @private)
        {
            UserId = userId;
            GitHubAccountId = gitHubAccountId;
            RepositoryId = repositoryId;
            Name = name;
            FullName = fullName;
            Private = @private;
            SelectedAt = DateTime.UtcNow;
        }
    }
}
