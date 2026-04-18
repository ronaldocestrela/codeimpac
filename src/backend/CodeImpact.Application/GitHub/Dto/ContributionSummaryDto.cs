namespace CodeImpact.Application.GitHub.Dto;

public sealed record ContributionSummaryDto(
    DateTime GeneratedAt,
    ContributionSummaryScopeDto Scope,
    ContributionSummaryMetricsDto Metrics,
    string ExecutiveSummary,
    IReadOnlyCollection<ContributionHighlightDto> Highlights,
    IReadOnlyCollection<ContributionSummaryEvidenceDto> Evidence);

public sealed record ContributionSummaryScopeDto(
    long? RepositoryId,
    DateTime? From,
    DateTime? To);

public sealed record ContributionSummaryMetricsDto(
    int CommitCount,
    int ApprovedPullRequestCount,
    int RepositoryCount);

public sealed record ContributionHighlightDto(
    string Title,
    string Insight,
    string Impact,
    IReadOnlyCollection<string> EvidenceIds);

public sealed record ContributionSummaryEvidenceDto(
    string EvidenceId,
    string EvidenceType,
    string RepositoryFullName,
    string ExternalReference,
    string Author,
    DateTime OccurredAt,
    string Status,
    string Url);