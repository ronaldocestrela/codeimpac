using CodeImpact.Domain.Common;

namespace CodeImpact.Domain.Entities;

public sealed class Plan : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public int RepositoriesLimit { get; private set; }
    public int ReportsPerMonth { get; private set; }
    public int RetentionDays { get; private set; }
    public bool IsActive { get; private set; }

    private Plan()
    {
    }

    public Plan(string name, string description, int repositoriesLimit, int reportsPerMonth, int retentionDays, bool isActive = true)
    {
        Name = name;
        Description = description;
        RepositoriesLimit = repositoriesLimit;
        ReportsPerMonth = reportsPerMonth;
        RetentionDays = retentionDays;
        IsActive = isActive;
    }

    public void Update(string description, int repositoriesLimit, int reportsPerMonth, int retentionDays, bool isActive)
    {
        Description = description;
        RepositoriesLimit = repositoriesLimit;
        ReportsPerMonth = reportsPerMonth;
        RetentionDays = retentionDays;
        IsActive = isActive;
        SetUpdated();
    }
}