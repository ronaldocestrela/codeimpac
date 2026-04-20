using CodeImpact.Domain.Common;

namespace CodeImpact.Domain.Entities;

public sealed class AdminAuditLog : BaseEntity
{
    public Guid AdminUserId { get; private set; }
    public string Action { get; private set; } = string.Empty;
    public string TargetType { get; private set; } = string.Empty;
    public string? TargetId { get; private set; }
    public string PayloadSummary { get; private set; } = string.Empty;
    public string Result { get; private set; } = string.Empty;
    public string? IpAddress { get; private set; }

    private AdminAuditLog()
    {
    }

    public AdminAuditLog(
        Guid adminUserId,
        string action,
        string targetType,
        string? targetId,
        string payloadSummary,
        string result,
        string? ipAddress)
    {
        AdminUserId = adminUserId;
        Action = action;
        TargetType = targetType;
        TargetId = targetId;
        PayloadSummary = payloadSummary;
        Result = result;
        IpAddress = ipAddress;
    }
}