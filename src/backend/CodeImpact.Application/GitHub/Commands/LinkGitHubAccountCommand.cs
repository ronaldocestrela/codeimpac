using System;
using CodeImpact.Application.GitHub.Dto;
using MediatR;

namespace CodeImpact.Application.GitHub.Commands
{
    public sealed record LinkGitHubAccountCommand(Guid UserId, string Code) : IRequest<GitHubAccountDto>;
}
