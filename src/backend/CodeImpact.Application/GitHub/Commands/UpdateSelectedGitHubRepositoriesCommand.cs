using System;
using System.Collections.Generic;
using CodeImpact.Application.GitHub.Dto;
using MediatR;

namespace CodeImpact.Application.GitHub.Commands
{
    public sealed record UpdateSelectedGitHubRepositoriesCommand(
        Guid UserId,
        IReadOnlyCollection<SelectedGitHubRepositoryDto> Repositories,
        string? OrganizationLogin) : IRequest;
}
