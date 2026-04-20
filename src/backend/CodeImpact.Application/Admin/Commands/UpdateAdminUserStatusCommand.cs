using MediatR;

namespace CodeImpact.Application.Admin.Commands;

public sealed record UpdateAdminUserStatusCommand(
    Guid AdminUserId,
    Guid UserId,
    string Status,
    string? Reason,
    string? IpAddress) : IRequest<bool>;