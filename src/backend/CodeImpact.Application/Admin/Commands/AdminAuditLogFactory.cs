using CodeImpact.Domain.Entities;

namespace CodeImpact.Application.Admin.Commands;

internal static class AdminAuditLogFactory
{
    public static AdminAuditLog Create(
        Guid adminUserId,
        string action,
        string targetType,
        string? targetId,
        string payloadSummary,
        string result,
        string? ipAddress)
    {
        return new AdminAuditLog(adminUserId, action, targetType, targetId, payloadSummary, result, ipAddress);
    }
}