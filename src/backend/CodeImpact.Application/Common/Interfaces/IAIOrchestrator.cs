using CodeImpact.Application.GitHub.Dto;

namespace CodeImpact.Application.Common.Interfaces;

public interface IAIOrchestrator
{
    Task<ContributionSummaryDto> GenerateContributionSummaryAsync(ContributionSummaryRequest request, CancellationToken cancellationToken = default);
}

public sealed record ContributionSummaryRequest(
    Guid UserId,
    long? RepositoryId,
    DateTime? From,
    DateTime? To);