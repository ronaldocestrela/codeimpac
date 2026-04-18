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

    public Task UpdateAsync(BackgroundJobExecution execution)
    {
        _dbContext.Update(execution);
        return _dbContext.SaveChangesAsync();
    }
}
