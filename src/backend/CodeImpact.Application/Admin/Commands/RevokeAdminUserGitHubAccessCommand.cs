using MediatR;

namespace CodeImpact.Application.Admin.Commands;

public sealed record RevokeAdminUserGitHubAccessCommand(
    Guid AdminUserId,
    Guid UserId,
    string? IpAddress) : IRequest<bool>;