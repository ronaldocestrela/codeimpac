using MediatR;

namespace CodeImpact.Application.Admin.Commands;

public sealed record RetryAdminJobCommand(
    Guid AdminUserId,
    Guid TaskId,
    string? IpAddress) : IRequest<Guid>;