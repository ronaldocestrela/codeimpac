using CodeImpact.Domain.Entities;
using CodeImpact.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace CodeImpact.Infrastructure.Persistence;

public sealed class UserUsageSnapshotRepository : IUserUsageSnapshotRepository
{
    private readonly CodeImpactDbContext _dbContext;

    public UserUsageSnapshotRepository(CodeImpactDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<UserUsageSnapshot?> GetByUserIdAsync(Guid userId)
    {
        return _dbContext.UserUsageSnapshots.FirstOrDefaultAsync(snapshot => snapshot.UserId == userId);
    }

    public async Task AddAsync(UserUsageSnapshot snapshot)
    {
        _dbContext.UserUsageSnapshots.Add(snapshot);
        await _dbContext.SaveChangesAsync();
    }

    public async Task UpdateAsync(UserUsageSnapshot snapshot)
    {
        _dbContext.UserUsageSnapshots.Update(snapshot);
        await _dbContext.SaveChangesAsync();
    }
}