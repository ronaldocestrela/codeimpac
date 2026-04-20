using CodeImpact.Domain.Entities;

namespace CodeImpact.Domain.Repositories;

public interface IAdminAuditLogRepository
{
    Task AddAsync(AdminAuditLog log);
    Task<IReadOnlyCollection<AdminAuditLog>> ListAsync(string? action, string? targetType, Guid? adminUserId, int page, int pageSize);
    Task<int> CountAsync(string? action, string? targetType, Guid? adminUserId);
}