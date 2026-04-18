namespace CodeImpact.Application.Reports.Dto;

public sealed record ExecutiveReportDto(
    Guid Id,
    DateTime GeneratedAt,
    ExecutiveReportScopeDto Scope,
    ExecutiveReportMetricsDto Metrics,
    string ExecutiveSummary,
    IReadOnlyCollection<ExecutiveReportHighlightDto> Highlights,
    IReadOnlyCollection<ExecutiveReportRiskDto> Risks,
    IReadOnlyCollection<ExecutiveReportEvidenceDto> Evidence);

public sealed record ExecutiveReportScopeDto(
    string DeveloperScope,
    long? RepositoryId,
    DateTime? From,
    DateTime? To,
    IReadOnlyCollection<string> Repositories);

public sealed record ExecutiveReportMetricsDto(
    int CommitCount,
    int PullRequestOpenCount,
    int PullRequestClosedCount,
    int PullRequestMergedCount,
    int PullRequestApprovedCount,
    double? AverageMergeLeadTimeHours,
    int RepositoryCount);

public sealed record ExecutiveReportHighlightDto(
    string Title,
    string Insight,
    string Impact,
    IReadOnlyCollection<string> EvidenceIds);

public sealed record ExecutiveReportRiskDto(
    string Risk,
    string Recommendation,
    IReadOnlyCollection<string> EvidenceIds);

public sealed record ExecutiveReportEvidenceDto(
    string EvidenceId,
    string EvidenceType,
    string RepositoryFullName,
    string ExternalReference,
    string Author,
    DateTime OccurredAt,
    string Status,
    string Url);
