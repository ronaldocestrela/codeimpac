namespace CodeImpact.Application.Admin.Dto;

public sealed record AdminUserSubscriptionDto(
    Guid UserId,
    Guid? SubscriptionId,
    Guid? PlanId,
    string? PlanName,
    string Status,
    DateTime? CurrentPeriodStart,
    DateTime? CurrentPeriodEnd,
    bool AutoRenew,
    string? BillingIssue,
    IReadOnlyCollection<AdminPlanOptionDto> AvailablePlans);

public sealed record AdminPlanOptionDto(
    Guid PlanId,
    string Name,
    string Description,
    int RepositoriesLimit,
    int ReportsPerMonth,
    int RetentionDays,
    bool IsActive);