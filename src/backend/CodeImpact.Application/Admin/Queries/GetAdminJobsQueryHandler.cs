using CodeImpact.Application.Admin.Dto;
using CodeImpact.Domain.Repositories;
using MediatR;

namespace CodeImpact.Application.Admin.Queries;

public sealed class GetAdminJobsQueryHandler : IRequestHandler<GetAdminJobsQuery, AdminJobListDto>
{
    private readonly IBackgroundJobExecutionRepository _jobRepository;

    public GetAdminJobsQueryHandler(IBackgroundJobExecutionRepository jobRepository)
    {
        _jobRepository = jobRepository;
    }

    public async Task<AdminJobListDto> Handle(GetAdminJobsQuery request, CancellationToken cancellationToken)
    {
        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize < 1 ? 20 : Math.Min(request.PageSize, 100);

        var items = await _jobRepository.ListAsync(request.JobType, request.Status, page, pageSize);
        var totalCount = await _jobRepository.CountAsync(request.JobType, request.Status);

        var dtoItems = items
            .Select(job => new AdminJobListItemDto(
                job.Id,
                job.UserId,
                job.JobType,
                job.Status,
                job.CreatedAt,
                job.StartedAt,
                job.CompletedAt,
                job.ErrorMessage,
                job.HangfireJobId))
            .ToArray();

        return new AdminJobListDto(dtoItems, totalCount, page, pageSize);
    }
}