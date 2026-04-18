using System.Collections.Generic;
using CodeImpact.Domain.Common;

namespace CodeImpact.Domain.Entities
{
    public class User : BaseEntity
    {
        public string Email { get; private set; } = string.Empty;

        public string FullName { get; private set; } = string.Empty;

        public string? ProfileImageUrl { get; private set; }

        public IReadOnlyCollection<UserOrganization> Organizations => _organizations.AsReadOnly();
        private readonly List<UserOrganization> _organizations = new();

        public IReadOnlyCollection<GitHubAccount> GitHubAccounts => _githubAccounts.AsReadOnly();
        private readonly List<GitHubAccount> _githubAccounts = new();

        private User() { }

        public User(string email, string fullName)
        {
            Email = email;
            FullName = fullName;
        }

        public void UpdateProfile(string fullName, string? profileImageUrl)
        {
            FullName = fullName;
            ProfileImageUrl = profileImageUrl;
            SetUpdated();
        }

        public void AddOrganization(UserOrganization membership)
        {
            _organizations.Add(membership);
        }

        public void AddGitHubAccount(GitHubAccount account)
        {
            _githubAccounts.Add(account);
        }
    }
}
