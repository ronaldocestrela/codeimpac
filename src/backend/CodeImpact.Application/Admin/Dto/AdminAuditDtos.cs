namespace CodeImpact.Application.Admin.Dto;

public sealed record AdminAuditLogDto(
    Guid Id,
    Guid AdminUserId,
    string Action,
    string TargetType,
    string? TargetId,
    string PayloadSummary,
    string Result,
    string? IpAddress,
    DateTime CreatedAt);

public sealed record AdminAuditLogListDto(
    IReadOnlyCollection<AdminAuditLogDto> Items,
    int TotalCount,
    int Page,
    int PageSize);