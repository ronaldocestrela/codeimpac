namespace CodeImpact.Application.Admin.Dto;

public sealed record AdminJobListItemDto(
    Guid TaskId,
    Guid UserId,
    string JobType,
    string Status,
    DateTime CreatedAt,
    DateTime? StartedAt,
    DateTime? CompletedAt,
    string? ErrorMessage,
    string? HangfireJobId);

public sealed record AdminJobListDto(
    IReadOnlyCollection<AdminJobListItemDto> Items,
    int TotalCount,
    int Page,
    int PageSize);