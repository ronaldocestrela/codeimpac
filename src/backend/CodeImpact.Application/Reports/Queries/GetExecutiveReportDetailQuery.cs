using CodeImpact.Application.Reports.Dto;
using MediatR;

namespace CodeImpact.Application.Reports.Queries;

public sealed record GetExecutiveReportDetailQuery(Guid UserId, Guid ReportId) : IRequest<ExecutiveReportDto?>;
