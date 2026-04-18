using CodeImpact.Domain.Common;

namespace CodeImpact.Domain.Entities;

public sealed class BackgroundJobExecution : BaseEntity
{
    public Guid UserId { get; private set; }
    public string JobType { get; private set; } = string.Empty;
    public string Status { get; private set; } = string.Empty;
    public string RequestJson { get; private set; } = string.Empty;
    public string? ResultJson { get; private set; }
    public string? ErrorMessage { get; private set; }
    public string? HangfireJobId { get; private set; }
    public DateTime? StartedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }

    private BackgroundJobExecution()
    {
    }

    public BackgroundJobExecution(Guid userId, string jobType, string requestJson)
    {
        UserId = userId;
        JobType = jobType;
        RequestJson = requestJson;
        Status = BackgroundJobExecutionStatus.Queued;
    }

    public void AssignHangfireJob(string hangfireJobId)
    {
        HangfireJobId = hangfireJobId;
        SetUpdated();
    }

    public void MarkProcessing()
    {
        Status = BackgroundJobExecutionStatus.Processing;
        StartedAt = DateTime.UtcNow;
        ErrorMessage = null;
        SetUpdated();
    }

    public void MarkSucceeded(string resultJson)
    {
        Status = BackgroundJobExecutionStatus.Succeeded;
        ResultJson = resultJson;
        ErrorMessage = null;
        CompletedAt = DateTime.UtcNow;
        SetUpdated();
    }

    public void MarkFailed(string errorMessage)
    {
        Status = BackgroundJobExecutionStatus.Failed;
        ErrorMessage = errorMessage;
        CompletedAt = DateTime.UtcNow;
        SetUpdated();
    }
}

public static class BackgroundJobExecutionType
{
    public const string ContributionSummary = "ContributionSummary";
    public const string ExecutiveReport = "ExecutiveReport";
}

public static class BackgroundJobExecutionStatus
{
    public const string Queued = "Queued";
    public const string Processing = "Processing";
    public const string Succeeded = "Succeeded";
    public const string Failed = "Failed";
}
