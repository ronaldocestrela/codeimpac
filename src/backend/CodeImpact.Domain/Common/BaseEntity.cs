using System;

namespace CodeImpact.Domain.Common
{
    public abstract class BaseEntity
    {
        public Guid Id { get; protected set; }

        public DateTime CreatedAt { get; protected set; }

        public DateTime? UpdatedAt { get; protected set; }

        protected BaseEntity()
        {
            Id = Guid.NewGuid();
            CreatedAt = DateTime.UtcNow;
        }

        public void SetUpdated()
        {
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
