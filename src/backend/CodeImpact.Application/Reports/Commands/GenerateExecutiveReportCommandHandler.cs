using CodeImpact.Application.Common.Interfaces;
using CodeImpact.Application.Reports.Dto;
using MediatR;

namespace CodeImpact.Application.Reports.Commands;

public sealed class GenerateExecutiveReportCommandHandler : IRequestHandler<GenerateExecutiveReportCommand, ExecutiveReportDto>
{
    private readonly IExecutiveReportOrchestrator _orchestrator;

    public GenerateExecutiveReportCommandHandler(IExecutiveReportOrchestrator orchestrator)
    {
        _orchestrator = orchestrator;
    }

    public Task<ExecutiveReportDto> Handle(GenerateExecutiveReportCommand request, CancellationToken cancellationToken)
    {
        return _orchestrator.GenerateAndPersistAsync(
            new ExecutiveReportRequest(request.UserId, request.RepositoryId, request.OrganizationLogin, request.From, request.To),
            cancellationToken);
    }
}
