using CodeImpact.Application.Common.Interfaces;
using CodeImpact.Domain.Entities;
using CodeImpact.Domain.Repositories;
using MediatR;

namespace CodeImpact.Application.Admin.Commands;

public sealed class RetryAdminJobCommandHandler : IRequestHandler<RetryAdminJobCommand, Guid>
{
    private readonly IBackgroundJobExecutionRepository _jobRepository;
    private readonly IBackgroundJobScheduler _scheduler;
    private readonly IAdminAuditLogRepository _auditRepository;

    public RetryAdminJobCommandHandler(
        IBackgroundJobExecutionRepository jobRepository,
        IBackgroundJobScheduler scheduler,
        IAdminAuditLogRepository auditRepository)
    {
        _jobRepository = jobRepository;
        _scheduler = scheduler;
        _auditRepository = auditRepository;
    }

    public async Task<Guid> Handle(RetryAdminJobCommand request, CancellationToken cancellationToken)
    {
        var currentJob = await _jobRepository.GetByIdAsync(request.TaskId);
        if (currentJob is null)
        {
            await _auditRepository.AddAsync(AdminAuditLogFactory.Create(
                request.AdminUserId,
                "RetryJob",
                "Job",
                request.TaskId.ToString(),
                "not-found",
                "failure",
                request.IpAddress));

            throw new InvalidOperationException("Job não encontrado.");
        }

        if (currentJob.Status != BackgroundJobExecutionStatus.Failed)
        {
            await _auditRepository.AddAsync(AdminAuditLogFactory.Create(
                request.AdminUserId,
                "RetryJob",
                "Job",
                request.TaskId.ToString(),
                $"invalid-status={currentJob.Status}",
                "failure",
                request.IpAddress));

            throw new InvalidOperationException("Apenas jobs com status Failed podem ser reprocessados.");
        }

        var retryJob = new BackgroundJobExecution(currentJob.UserId, currentJob.JobType, currentJob.RequestJson);
        await _jobRepository.AddAsync(retryJob);

        var hangfireJobId = retryJob.JobType switch
        {
            BackgroundJobExecutionType.ExecutiveReport => _scheduler.EnqueueExecutiveReportJob(retryJob.Id),
            BackgroundJobExecutionType.ContributionSummary => _scheduler.EnqueueContributionSummaryJob(retryJob.Id),
            _ => throw new InvalidOperationException("Tipo de job não suportado para retry.")
        };

        retryJob.AssignHangfireJob(hangfireJobId);
        await _jobRepository.UpdateAsync(retryJob);

        await _auditRepository.AddAsync(AdminAuditLogFactory.Create(
            request.AdminUserId,
            "RetryJob",
            "Job",
            request.TaskId.ToString(),
            $"newTaskId={retryJob.Id}",
            "success",
            request.IpAddress));

        return retryJob.Id;
    }
}