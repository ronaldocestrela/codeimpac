using CodeImpact.Domain.Entities;
using CodeImpact.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace CodeImpact.Infrastructure.Persistence;

public sealed class PlanRepository : IPlanRepository
{
    private readonly CodeImpactDbContext _dbContext;

    public PlanRepository(CodeImpactDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyCollection<Plan>> ListActiveAsync()
    {
        return await _dbContext.Plans
            .AsNoTracking()
            .Where(plan => plan.IsActive)
            .OrderBy(plan => plan.Name)
            .ToListAsync();
    }

    public Task<Plan?> GetByIdAsync(Guid id)
    {
        return _dbContext.Plans.FirstOrDefaultAsync(plan => plan.Id == id);
    }

    public Task<Plan?> GetByNameAsync(string name)
    {
        return _dbContext.Plans.FirstOrDefaultAsync(plan => plan.Name == name);
    }

    public async Task AddAsync(Plan plan)
    {
        _dbContext.Plans.Add(plan);
        await _dbContext.SaveChangesAsync();
    }
}