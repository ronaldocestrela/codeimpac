using CodeImpact.Application.Reports.Dto;
using MediatR;

namespace CodeImpact.Application.Reports.Queries;

public sealed record GetExecutiveReportsQuery(
    Guid UserId,
    long? RepositoryId,
    string? OrganizationLogin,
    DateTime? From,
    DateTime? To) : IRequest<IReadOnlyCollection<ExecutiveReportListItemDto>>;
