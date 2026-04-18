using CodeImpact.Application.AI;
using CodeImpact.Application.Reports.Dto;
using CodeImpact.Domain.Repositories;
using MediatR;

namespace CodeImpact.Application.Reports.Queries;

public sealed class ExportExecutiveReportQueryHandler : IRequestHandler<ExportExecutiveReportQuery, ExecutiveReportExportFileDto?>
{
    private readonly IReportRepository _reportRepository;
    private readonly IExecutiveReportExportService _exportService;

    public ExportExecutiveReportQueryHandler(IReportRepository reportRepository, IExecutiveReportExportService exportService)
    {
        _reportRepository = reportRepository;
        _exportService = exportService;
    }

    public async Task<ExecutiveReportExportFileDto?> Handle(ExportExecutiveReportQuery request, CancellationToken cancellationToken)
    {
        var report = await _reportRepository.GetByIdAsync(request.UserId, request.ReportId);
        if (report is null)
        {
            return null;
        }

        var reportDto = ExecutiveReportOrchestrator.MapToDto(report);
        return _exportService.Build(reportDto, request.Format);
    }
}
