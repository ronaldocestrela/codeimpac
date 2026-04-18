using CodeImpact.Application.Reports.Dto;
using MediatR;

namespace CodeImpact.Application.Reports.Queries;

public sealed record GetExecutiveReportsQuery(
    Guid UserId,
    long? RepositoryId,
    DateTime? From,
    DateTime? To) : IRequest<IReadOnlyCollection<ExecutiveReportListItemDto>>;
