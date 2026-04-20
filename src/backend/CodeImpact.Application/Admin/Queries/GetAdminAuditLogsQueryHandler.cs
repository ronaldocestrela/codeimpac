using CodeImpact.Application.Admin.Dto;
using CodeImpact.Domain.Repositories;
using MediatR;

namespace CodeImpact.Application.Admin.Queries;

public sealed class GetAdminAuditLogsQueryHandler : IRequestHandler<GetAdminAuditLogsQuery, AdminAuditLogListDto>
{
    private readonly IAdminAuditLogRepository _auditLogRepository;

    public GetAdminAuditLogsQueryHandler(IAdminAuditLogRepository auditLogRepository)
    {
        _auditLogRepository = auditLogRepository;
    }

    public async Task<AdminAuditLogListDto> Handle(GetAdminAuditLogsQuery request, CancellationToken cancellationToken)
    {
        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize < 1 ? 20 : Math.Min(request.PageSize, 100);

        var logs = await _auditLogRepository.ListAsync(request.Action, request.TargetType, request.AdminUserId, page, pageSize);
        var total = await _auditLogRepository.CountAsync(request.Action, request.TargetType, request.AdminUserId);

        var items = logs
            .Select(log => new AdminAuditLogDto(
                log.Id,
                log.AdminUserId,
                log.Action,
                log.TargetType,
                log.TargetId,
                log.PayloadSummary,
                log.Result,
                log.IpAddress,
                log.CreatedAt))
            .ToArray();

        return new AdminAuditLogListDto(items, total, page, pageSize);
    }
}