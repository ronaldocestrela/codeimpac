using CodeImpact.Domain.Entities;
using CodeImpact.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace CodeImpact.Infrastructure.Persistence;

public sealed class AdminAuditLogRepository : IAdminAuditLogRepository
{
    private readonly CodeImpactDbContext _dbContext;

    public AdminAuditLogRepository(CodeImpactDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(AdminAuditLog log)
    {
        _dbContext.AdminAuditLogs.Add(log);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<IReadOnlyCollection<AdminAuditLog>> ListAsync(string? action, string? targetType, Guid? adminUserId, int page, int pageSize)
    {
        return await ApplyFilters(_dbContext.AdminAuditLogs.AsNoTracking(), action, targetType, adminUserId)
            .OrderByDescending(log => log.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public Task<int> CountAsync(string? action, string? targetType, Guid? adminUserId)
    {
        return ApplyFilters(_dbContext.AdminAuditLogs.AsNoTracking(), action, targetType, adminUserId)
            .CountAsync();
    }

    private static IQueryable<AdminAuditLog> ApplyFilters(IQueryable<AdminAuditLog> query, string? action, string? targetType, Guid? adminUserId)
    {
        if (!string.IsNullOrWhiteSpace(action))
        {
            query = query.Where(log => log.Action == action);
        }

        if (!string.IsNullOrWhiteSpace(targetType))
        {
            query = query.Where(log => log.TargetType == targetType);
        }

        if (adminUserId.HasValue)
        {
            query = query.Where(log => log.AdminUserId == adminUserId.Value);
        }

        return query;
    }
}