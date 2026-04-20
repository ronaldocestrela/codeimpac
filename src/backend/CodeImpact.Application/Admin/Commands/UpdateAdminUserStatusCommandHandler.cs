using CodeImpact.Application.Common.Interfaces;
using CodeImpact.Domain.Repositories;
using MediatR;

namespace CodeImpact.Application.Admin.Commands;

public sealed class UpdateAdminUserStatusCommandHandler : IRequestHandler<UpdateAdminUserStatusCommand, bool>
{
    private readonly IAdminUserDirectory _directory;
    private readonly IAdminAuditLogRepository _auditRepository;

    public UpdateAdminUserStatusCommandHandler(IAdminUserDirectory directory, IAdminAuditLogRepository auditRepository)
    {
        _directory = directory;
        _auditRepository = auditRepository;
    }

    public async Task<bool> Handle(UpdateAdminUserStatusCommand request, CancellationToken cancellationToken)
    {
        var updated = await _directory.UpdateStatusAsync(request.UserId, request.Status);

        await _auditRepository.AddAsync(AdminAuditLogFactory.Create(
            request.AdminUserId,
            "UpdateUserStatus",
            "User",
            request.UserId.ToString(),
            $"status={request.Status};reason={request.Reason}",
            updated ? "success" : "failure",
            request.IpAddress));

        return updated;
    }
}