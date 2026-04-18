using CodeImpact.Application.AI;
using CodeImpact.Application.Reports.Dto;
using CodeImpact.Domain.Repositories;
using MediatR;

namespace CodeImpact.Application.Reports.Queries;

public sealed class GetExecutiveReportDetailQueryHandler : IRequestHandler<GetExecutiveReportDetailQuery, ExecutiveReportDto?>
{
    private readonly IReportRepository _reportRepository;

    public GetExecutiveReportDetailQueryHandler(IReportRepository reportRepository)
    {
        _reportRepository = reportRepository;
    }

    public async Task<ExecutiveReportDto?> Handle(GetExecutiveReportDetailQuery request, CancellationToken cancellationToken)
    {
        var report = await _reportRepository.GetByIdAsync(request.UserId, request.ReportId);
        return report is null ? null : ExecutiveReportOrchestrator.MapToDto(report);
    }
}
