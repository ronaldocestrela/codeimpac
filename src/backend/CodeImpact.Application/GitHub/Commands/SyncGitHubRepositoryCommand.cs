using System;
using MediatR;

namespace CodeImpact.Application.GitHub.Commands
{
    public sealed record SyncGitHubRepositoryCommand(Guid UserId, long RepositoryId) : IRequest;
}
