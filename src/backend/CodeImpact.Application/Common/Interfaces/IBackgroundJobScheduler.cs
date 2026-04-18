namespace CodeImpact.Application.Common.Interfaces;

public interface IBackgroundJobScheduler
{
    string EnqueueContributionSummaryJob(Guid taskId);
    string EnqueueExecutiveReportJob(Guid taskId);
}
