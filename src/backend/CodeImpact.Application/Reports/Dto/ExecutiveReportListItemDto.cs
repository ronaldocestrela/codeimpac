namespace CodeImpact.Application.Reports.Dto;

public sealed record ExecutiveReportListItemDto(
    Guid Id,
    DateTime GeneratedAt,
    long? RepositoryId,
    DateTime? From,
    DateTime? To,
    int CommitCount,
    int PullRequestApprovedCount,
    int RepositoryCount,
    string ExecutiveSummaryPreview);
