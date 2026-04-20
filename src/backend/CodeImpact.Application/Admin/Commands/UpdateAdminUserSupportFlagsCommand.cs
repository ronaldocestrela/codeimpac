using MediatR;

namespace CodeImpact.Application.Admin.Commands;

public sealed record UpdateAdminUserSupportFlagsCommand(
    Guid AdminUserId,
    Guid UserId,
    string[] SupportFlags,
    string? IpAddress) : IRequest<bool>;