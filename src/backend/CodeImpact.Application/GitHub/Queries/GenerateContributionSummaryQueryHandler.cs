using CodeImpact.Application.Common.Interfaces;
using CodeImpact.Application.GitHub.Dto;
using MediatR;

namespace CodeImpact.Application.GitHub.Queries;

public sealed class GenerateContributionSummaryQueryHandler : IRequestHandler<GenerateContributionSummaryQuery, ContributionSummaryDto>
{
    private readonly IAIOrchestrator _aiOrchestrator;

    public GenerateContributionSummaryQueryHandler(IAIOrchestrator aiOrchestrator)
    {
        _aiOrchestrator = aiOrchestrator;
    }

    public Task<ContributionSummaryDto> Handle(GenerateContributionSummaryQuery request, CancellationToken cancellationToken)
    {
        return _aiOrchestrator.GenerateContributionSummaryAsync(
            new ContributionSummaryRequest(request.UserId, request.RepositoryId, request.From, request.To),
            cancellationToken);
    }
}