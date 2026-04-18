using CodeImpact.Application.GitHub.Dto;
using MediatR;

namespace CodeImpact.Application.GitHub.Queries;

public sealed record GenerateContributionSummaryQuery(
    Guid UserId,
    long? RepositoryId,
    DateTime? From,
    DateTime? To) : IRequest<ContributionSummaryDto>;