using System;
using System.Collections.Generic;
using CodeImpact.Application.GitHub.Dto;
using MediatR;

namespace CodeImpact.Application.GitHub.Queries
{
    public sealed record GetGitHubRepositoriesQuery(Guid UserId) : IRequest<IEnumerable<GitHubRepositoryDto>>;
}
