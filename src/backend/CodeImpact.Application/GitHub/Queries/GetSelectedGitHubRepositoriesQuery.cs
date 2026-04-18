using System;
using System.Collections.Generic;
using CodeImpact.Application.GitHub.Dto;
using MediatR;

namespace CodeImpact.Application.GitHub.Queries
{
    public sealed record GetSelectedGitHubRepositoriesQuery(Guid UserId) : IRequest<IReadOnlyCollection<SelectedGitHubRepositoryDto>>;
}
