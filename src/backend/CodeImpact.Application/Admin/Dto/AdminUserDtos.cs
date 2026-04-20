namespace CodeImpact.Application.Admin.Dto;

public sealed record AdminUserListItemDto(
    Guid UserId,
    string Email,
    string FullName,
    string AccountStatus,
    string[] Roles,
    string[] SupportFlags,
    DateTime CreatedAt,
    DateTime? LastLoginAt,
    string? SubscriptionStatus,
    string? PlanName,
    int RepositoriesUsed,
    int ReportsUsedThisMonth);

public sealed record AdminUserListDto(
    IReadOnlyCollection<AdminUserListItemDto> Items,
    int TotalCount,
    int Page,
    int PageSize);

public sealed record AdminUserDetailDto(
    AdminUserListItemDto User,
    DateTime? LastSyncAt);