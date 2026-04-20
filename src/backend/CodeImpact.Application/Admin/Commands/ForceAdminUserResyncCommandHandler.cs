using CodeImpact.Application.GitHub.Commands;
using CodeImpact.Domain.Repositories;
using MediatR;

namespace CodeImpact.Application.Admin.Commands;

public sealed class ForceAdminUserResyncCommandHandler : IRequestHandler<ForceAdminUserResyncCommand, int>
{
    private readonly IGitHubRepositorySelectionRepository _selectionRepository;
    private readonly ISender _sender;
    private readonly IAdminAuditLogRepository _auditRepository;

    public ForceAdminUserResyncCommandHandler(
        IGitHubRepositorySelectionRepository selectionRepository,
        ISender sender,
        IAdminAuditLogRepository auditRepository)
    {
        _selectionRepository = selectionRepository;
        _sender = sender;
        _auditRepository = auditRepository;
    }

    public async Task<int> Handle(ForceAdminUserResyncCommand request, CancellationToken cancellationToken)
    {
        var selections = await _selectionRepository.GetByUserIdAsync(request.UserId);
        var syncedCount = 0;

        foreach (var selection in selections)
        {
            await _sender.Send(new SyncGitHubRepositoryCommand(request.UserId, selection.RepositoryId), cancellationToken);
            syncedCount++;
        }

        await _auditRepository.AddAsync(AdminAuditLogFactory.Create(
            request.AdminUserId,
            "ForceUserResync",
            "User",
            request.UserId.ToString(),
            $"repositoriesSynced={syncedCount}",
            "success",
            request.IpAddress));

        return syncedCount;
    }
}