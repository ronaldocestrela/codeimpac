using System.Collections.Generic;
using CodeImpact.Domain.Common;

namespace CodeImpact.Domain.Entities
{
    public class Organization : BaseEntity
    {
        public string Name { get; private set; } = string.Empty;

        public IReadOnlyCollection<UserOrganization> Members => _members.AsReadOnly();
        private readonly List<UserOrganization> _members = new();

        private Organization() { }

        public Organization(string name)
        {
            Name = name;
        }

        public void UpdateName(string name)
        {
            Name = name;
            SetUpdated();
        }

        public void AddMember(UserOrganization membership)
        {
            _members.Add(membership);
        }
    }
}
