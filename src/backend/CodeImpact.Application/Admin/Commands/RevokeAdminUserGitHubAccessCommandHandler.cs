using CodeImpact.Domain.Repositories;
using MediatR;

namespace CodeImpact.Application.Admin.Commands;

public sealed class RevokeAdminUserGitHubAccessCommandHandler : IRequestHandler<RevokeAdminUserGitHubAccessCommand, bool>
{
    private readonly IGitHubAccountRepository _gitHubAccountRepository;
    private readonly IAdminAuditLogRepository _auditRepository;

    public RevokeAdminUserGitHubAccessCommandHandler(
        IGitHubAccountRepository gitHubAccountRepository,
        IAdminAuditLogRepository auditRepository)
    {
        _gitHubAccountRepository = gitHubAccountRepository;
        _auditRepository = auditRepository;
    }

    public async Task<bool> Handle(RevokeAdminUserGitHubAccessCommand request, CancellationToken cancellationToken)
    {
        var account = await _gitHubAccountRepository.GetByUserIdAsync(request.UserId);
        if (account is null)
        {
            await _auditRepository.AddAsync(AdminAuditLogFactory.Create(
                request.AdminUserId,
                "RevokeGitHubAccess",
                "User",
                request.UserId.ToString(),
                "github-account-not-found",
                "failure",
                request.IpAddress));

            return false;
        }

        await _gitHubAccountRepository.DeleteAsync(account);
        await _auditRepository.AddAsync(AdminAuditLogFactory.Create(
            request.AdminUserId,
            "RevokeGitHubAccess",
            "User",
            request.UserId.ToString(),
            "github-account-removed",
            "success",
            request.IpAddress));

        return true;
    }
}