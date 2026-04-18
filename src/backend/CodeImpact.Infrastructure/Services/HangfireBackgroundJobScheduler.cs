using CodeImpact.Application.BackgroundJobs;
using CodeImpact.Application.Common.Interfaces;
using Hangfire;

namespace CodeImpact.Infrastructure.Services;

public sealed class HangfireBackgroundJobScheduler : IBackgroundJobScheduler
{
    private readonly IBackgroundJobClient _backgroundJobClient;

    public HangfireBackgroundJobScheduler(IBackgroundJobClient backgroundJobClient)
    {
        _backgroundJobClient = backgroundJobClient;
    }

    public string EnqueueContributionSummaryJob(Guid taskId)
    {
        return _backgroundJobClient.Enqueue<BackgroundGenerationJobProcessor>(processor => processor.ProcessContributionSummaryAsync(taskId));
    }

    public string EnqueueExecutiveReportJob(Guid taskId)
    {
        return _backgroundJobClient.Enqueue<BackgroundGenerationJobProcessor>(processor => processor.ProcessExecutiveReportAsync(taskId));
    }
}
