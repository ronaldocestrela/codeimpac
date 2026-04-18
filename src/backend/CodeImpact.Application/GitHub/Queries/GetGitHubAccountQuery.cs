using CodeImpact.Application.GitHub.Dto;
using MediatR;

namespace CodeImpact.Application.GitHub.Queries;

public sealed record GetGitHubAccountQuery(Guid UserId) : IRequest<GitHubAccountDto?>;
