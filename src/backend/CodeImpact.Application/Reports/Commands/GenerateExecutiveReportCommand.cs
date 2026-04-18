using CodeImpact.Application.Reports.Dto;
using MediatR;

namespace CodeImpact.Application.Reports.Commands;

public sealed record GenerateExecutiveReportCommand(
    Guid UserId,
    long? RepositoryId,
    DateTime? From,
    DateTime? To) : IRequest<ExecutiveReportDto>;
