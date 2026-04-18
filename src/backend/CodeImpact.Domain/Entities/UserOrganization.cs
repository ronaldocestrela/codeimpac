using System;
using CodeImpact.Domain.Common;

namespace CodeImpact.Domain.Entities
{
    public enum OrganizationRole
    {
        Owner,
        Admin,
        Member
    }

    public class UserOrganization : BaseEntity
    {
        public Guid UserId { get; private set; }

        public Guid OrganizationId { get; private set; }

        public OrganizationRole Role { get; private set; }

        public DateTime JoinedAt { get; private set; }

        private UserOrganization() { }

        public UserOrganization(Guid userId, Guid organizationId, OrganizationRole role)
        {
            UserId = userId;
            OrganizationId = organizationId;
            Role = role;
            JoinedAt = DateTime.UtcNow;
        }

        public void ChangeRole(OrganizationRole role)
        {
            Role = role;
            SetUpdated();
        }
    }
}
