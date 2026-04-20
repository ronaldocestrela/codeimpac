using CodeImpact.Application.Common.Interfaces;
using CodeImpact.Domain.Repositories;
using MediatR;

namespace CodeImpact.Application.Admin.Commands;

public sealed class UpdateAdminUserSupportFlagsCommandHandler : IRequestHandler<UpdateAdminUserSupportFlagsCommand, bool>
{
    private readonly IAdminUserDirectory _directory;
    private readonly IAdminAuditLogRepository _auditRepository;

    public UpdateAdminUserSupportFlagsCommandHandler(IAdminUserDirectory directory, IAdminAuditLogRepository auditRepository)
    {
        _directory = directory;
        _auditRepository = auditRepository;
    }

    public async Task<bool> Handle(UpdateAdminUserSupportFlagsCommand request, CancellationToken cancellationToken)
    {
        var updated = await _directory.UpdateSupportFlagsAsync(request.UserId, request.SupportFlags);

        await _auditRepository.AddAsync(AdminAuditLogFactory.Create(
            request.AdminUserId,
            "UpdateUserSupportFlags",
            "User",
            request.UserId.ToString(),
            string.Join(',', request.SupportFlags),
            updated ? "success" : "failure",
            request.IpAddress));

        return updated;
    }
}