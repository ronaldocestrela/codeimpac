using System.Text.Json;
using CodeImpact.Application.BackgroundJobs;
using CodeImpact.Application.BackgroundJobs.Dto;
using CodeImpact.Application.Common.Interfaces;
using CodeImpact.Domain.Entities;
using CodeImpact.Domain.Repositories;
using MediatR;

namespace CodeImpact.Application.BackgroundJobs.Commands;

public sealed class EnqueueExecutiveReportJobCommandHandler : IRequestHandler<EnqueueExecutiveReportJobCommand, BackgroundJobEnqueueDto>
{
    private readonly IBackgroundJobExecutionRepository _jobRepository;
    private readonly IBackgroundJobScheduler _scheduler;

    public EnqueueExecutiveReportJobCommandHandler(IBackgroundJobExecutionRepository jobRepository, IBackgroundJobScheduler scheduler)
    {
        _jobRepository = jobRepository;
        _scheduler = scheduler;
    }

    public async Task<BackgroundJobEnqueueDto> Handle(EnqueueExecutiveReportJobCommand request, CancellationToken cancellationToken)
    {
        var payload = new ExecutiveReportJobRequest(request.RepositoryId, request.From, request.To);
        var execution = new BackgroundJobExecution(
            request.UserId,
            BackgroundJobExecutionType.ExecutiveReport,
            JsonSerializer.Serialize(payload));

        await _jobRepository.AddAsync(execution);

        var hangfireJobId = _scheduler.EnqueueExecutiveReportJob(execution.Id);
        execution.AssignHangfireJob(hangfireJobId);
        await _jobRepository.UpdateAsync(execution);

        return new BackgroundJobEnqueueDto(
            execution.Id,
            execution.JobType,
            execution.Status,
            execution.HangfireJobId,
            execution.CreatedAt);
    }
}
