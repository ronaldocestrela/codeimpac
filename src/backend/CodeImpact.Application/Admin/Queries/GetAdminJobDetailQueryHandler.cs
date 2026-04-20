using CodeImpact.Application.Admin.Dto;
using CodeImpact.Domain.Repositories;
using MediatR;

namespace CodeImpact.Application.Admin.Queries;

public sealed class GetAdminJobDetailQueryHandler : IRequestHandler<GetAdminJobDetailQuery, AdminJobListItemDto?>
{
    private readonly IBackgroundJobExecutionRepository _jobRepository;

    public GetAdminJobDetailQueryHandler(IBackgroundJobExecutionRepository jobRepository)
    {
        _jobRepository = jobRepository;
    }

    public async Task<AdminJobListItemDto?> Handle(GetAdminJobDetailQuery request, CancellationToken cancellationToken)
    {
        var job = await _jobRepository.GetByIdAsync(request.TaskId);
        if (job is null)
        {
            return null;
        }

        return new AdminJobListItemDto(
            job.Id,
            job.UserId,
            job.JobType,
            job.Status,
            job.CreatedAt,
            job.StartedAt,
            job.CompletedAt,
            job.ErrorMessage,
            job.HangfireJobId);
    }
}