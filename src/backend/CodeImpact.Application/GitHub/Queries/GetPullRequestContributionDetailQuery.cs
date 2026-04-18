using CodeImpact.Application.GitHub.Dto;
using MediatR;

namespace CodeImpact.Application.GitHub.Queries;

public sealed record GetPullRequestContributionDetailQuery(Guid UserId, Guid ContributionId) : IRequest<ContributionDetailDto?>;
