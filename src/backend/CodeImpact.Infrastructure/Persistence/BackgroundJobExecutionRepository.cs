using CodeImpact.Domain.Entities;
using CodeImpact.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace CodeImpact.Infrastructure.Persistence;

public sealed class BackgroundJobExecutionRepository : IBackgroundJobExecutionRepository
{
    private readonly CodeImpactDbContext _dbContext;

    public BackgroundJobExecutionRepository(CodeImpactDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(BackgroundJobExecution execution)
    {
        _dbContext.Add(execution);
        await _dbContext.SaveChangesAsync();
    }

    public Task<BackgroundJobExecution?> GetByIdAsync(Guid id)
    {
        return _dbContext.Set<BackgroundJobExecution>()
            .FirstOrDefaultAsync(job => job.Id == id);
    }

    public Task<BackgroundJobExecution?> GetByIdForUserAsync(Guid userId, Guid id)
    {
        return _dbContext.Set<BackgroundJobExecution>()
            .AsNoTracking()
            .FirstOrDefaultAsync(job => job.UserId == userId && job.Id == id);
    }

    public async Task<IReadOnlyCollection<BackgroundJobExecution>> ListAsync(string? jobType, string? status, int page, int pageSize)
    {
        var query = ApplyFilters(_dbContext.Set<BackgroundJobExecution>().AsNoTracking(), jobType, status);

        return await query
            .OrderByDescending(job => job.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public Task<int> CountAsync(string? jobType, string? status)
    {
        return ApplyFilters(_dbContext.Set<BackgroundJobExecution>().AsNoTracking(), jobType, status)
            .CountAsync();
    }

    public Task UpdateAsync(BackgroundJobExecution execution)
    {
        _dbContext.Update(execution);
        return _dbContext.SaveChangesAsync();
    }

    private static IQueryable<BackgroundJobExecution> ApplyFilters(IQueryable<BackgroundJobExecution> query, string? jobType, string? status)
    {
        if (!string.IsNullOrWhiteSpace(jobType))
        {
            query = query.Where(job => job.JobType == jobType);
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(job => job.Status == status);
        }

        return query;
    }
}
