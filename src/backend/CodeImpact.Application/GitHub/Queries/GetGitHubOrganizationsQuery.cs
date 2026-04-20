using System;
using System.Collections.Generic;
using CodeImpact.Application.GitHub.Dto;
using MediatR;

namespace CodeImpact.Application.GitHub.Queries
{
    public sealed record GetGitHubOrganizationsQuery(Guid UserId) : IRequest<IEnumerable<GitHubOrganizationDto>>;
}