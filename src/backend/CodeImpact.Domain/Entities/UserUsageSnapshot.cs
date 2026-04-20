using CodeImpact.Domain.Common;

namespace CodeImpact.Domain.Entities;

public sealed class UserUsageSnapshot : BaseEntity
{
    public Guid UserId { get; private set; }
    public int RepositoriesUsed { get; private set; }
    public int ReportsUsedThisMonth { get; private set; }
    public DateTime? LastSyncAt { get; private set; }

    private UserUsageSnapshot()
    {
    }

    public UserUsageSnapshot(Guid userId, int repositoriesUsed, int reportsUsedThisMonth, DateTime? lastSyncAt)
    {
        UserId = userId;
        RepositoriesUsed = repositoriesUsed;
        ReportsUsedThisMonth = reportsUsedThisMonth;
        LastSyncAt = lastSyncAt;
    }

    public void Update(int repositoriesUsed, int reportsUsedThisMonth, DateTime? lastSyncAt)
    {
        RepositoriesUsed = repositoriesUsed;
        ReportsUsedThisMonth = reportsUsedThisMonth;
        LastSyncAt = lastSyncAt;
        SetUpdated();
    }
}