using CodeImpact.Application.Reports.Dto;
using MediatR;

namespace CodeImpact.Application.Reports.Queries;

public sealed record ExportExecutiveReportQuery(
    Guid UserId,
    Guid ReportId,
    ExecutiveReportExportFormat Format) : IRequest<ExecutiveReportExportFileDto?>;
