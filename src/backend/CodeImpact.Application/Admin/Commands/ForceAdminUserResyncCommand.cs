using MediatR;

namespace CodeImpact.Application.Admin.Commands;

public sealed record ForceAdminUserResyncCommand(
    Guid AdminUserId,
    Guid UserId,
    string? IpAddress) : IRequest<int>;