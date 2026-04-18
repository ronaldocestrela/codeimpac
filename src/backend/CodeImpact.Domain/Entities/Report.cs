using CodeImpact.Domain.Common;

namespace CodeImpact.Domain.Entities;

public class Report : BaseEntity
{
    public Guid UserId { get; private set; }
    public long? RepositoryId { get; private set; }
    public DateTime? FromDate { get; private set; }
    public DateTime? ToDate { get; private set; }
    public string DeveloperScope { get; private set; } = string.Empty;
    public string RepositoriesJson { get; private set; } = "[]";
    public int CommitCount { get; private set; }
    public int PullRequestOpenCount { get; private set; }
    public int PullRequestClosedCount { get; private set; }
    public int PullRequestMergedCount { get; private set; }
    public int PullRequestApprovedCount { get; private set; }
    public double? AverageMergeLeadTimeHours { get; private set; }
    public string ExecutiveSummary { get; private set; } = string.Empty;
    public string HighlightsJson { get; private set; } = "[]";
    public string RisksJson { get; private set; } = "[]";
    public string EvidenceJson { get; private set; } = "[]";
    public DateTime GeneratedAt { get; private set; }

    private Report() { }

    public Report(
        Guid userId,
        long? repositoryId,
        DateTime? fromDate,
        DateTime? toDate,
        string developerScope,
        string repositoriesJson,
        int commitCount,
        int pullRequestOpenCount,
        int pullRequestClosedCount,
        int pullRequestMergedCount,
        int pullRequestApprovedCount,
        double? averageMergeLeadTimeHours,
        string executiveSummary,
        string highlightsJson,
        string risksJson,
        string evidenceJson,
        DateTime generatedAt)
    {
        UserId = userId;
        RepositoryId = repositoryId;
        FromDate = fromDate;
        ToDate = toDate;
        DeveloperScope = developerScope;
        RepositoriesJson = repositoriesJson;
        CommitCount = commitCount;
        PullRequestOpenCount = pullRequestOpenCount;
        PullRequestClosedCount = pullRequestClosedCount;
        PullRequestMergedCount = pullRequestMergedCount;
        PullRequestApprovedCount = pullRequestApprovedCount;
        AverageMergeLeadTimeHours = averageMergeLeadTimeHours;
        ExecutiveSummary = executiveSummary;
        HighlightsJson = highlightsJson;
        RisksJson = risksJson;
        EvidenceJson = evidenceJson;
        GeneratedAt = generatedAt;
    }
}
