using CodeImpact.Application.GitHub.Dto;

namespace CodeImpact.Application.BackgroundJobs.Dto;

public sealed record BackgroundJobEnqueueDto(
    Guid TaskId,
    string JobType,
    string Status,
    string? HangfireJobId,
    DateTime CreatedAt);

public sealed record BackgroundJobStatusDto(
    Guid TaskId,
    string JobType,
    string Status,
    DateTime CreatedAt,
    DateTime? StartedAt,
    DateTime? CompletedAt,
    string? ErrorMessage,
    string? HangfireJobId,
    Guid? ReportId,
    ContributionSummaryDto? ContributionSummary);
