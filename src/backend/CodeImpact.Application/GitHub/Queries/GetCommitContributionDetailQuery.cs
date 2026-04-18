using CodeImpact.Application.GitHub.Dto;
using MediatR;

namespace CodeImpact.Application.GitHub.Queries;

public sealed record GetCommitContributionDetailQuery(Guid UserId, Guid ContributionId) : IRequest<ContributionDetailDto?>;
