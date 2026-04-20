using CodeImpact.Application.Admin.Dto;
using MediatR;

namespace CodeImpact.Application.Admin.Queries;

public sealed record GetAdminAuditLogsQuery(
    string? Action,
    string? TargetType,
    Guid? AdminUserId,
    int Page,
    int PageSize) : IRequest<AdminAuditLogListDto>;