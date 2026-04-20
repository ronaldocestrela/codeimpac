using CodeImpact.Domain.Entities;
using CodeImpact.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace CodeImpact.Infrastructure.Persistence;

public class ReportRepository : IReportRepository
{
    private readonly CodeImpactDbContext _dbContext;

    public ReportRepository(CodeImpactDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(Report report)
    {
        _dbContext.Add(report);
        await _dbContext.SaveChangesAsync();
    }

    public Task<Report?> GetByIdAsync(Guid userId, Guid reportId)
    {
        return _dbContext.Set<Report>()
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.UserId == userId && r.Id == reportId);
    }

    public async Task<IReadOnlyCollection<Report>> ListByUserAsync(Guid userId, long? repositoryId, DateTime? from, DateTime? to)
    {
        var query = _dbContext.Set<Report>()
            .AsNoTracking()
            .Where(r => r.UserId == userId);

        if (repositoryId.HasValue)
        {
            query = query.Where(r => r.RepositoryId == repositoryId);
        }

        if (from.HasValue)
        {
            query = query.Where(r => !r.ToDate.HasValue || r.ToDate.Value >= from.Value);
        }

        if (to.HasValue)
        {
            query = query.Where(r => !r.FromDate.HasValue || r.FromDate.Value <= to.Value);
        }

        return await query
            .OrderByDescending(r => r.GeneratedAt)
            .ToListAsync();
    }
}
