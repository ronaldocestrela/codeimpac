using CodeImpact.Domain.Entities;
using CodeImpact.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace CodeImpact.Infrastructure.Persistence;

public sealed class UserSubscriptionRepository : IUserSubscriptionRepository
{
    private readonly CodeImpactDbContext _dbContext;

    public UserSubscriptionRepository(CodeImpactDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<UserSubscription?> GetByUserIdAsync(Guid userId)
    {
        return _dbContext.UserSubscriptions.FirstOrDefaultAsync(subscription => subscription.UserId == userId);
    }

    public async Task AddAsync(UserSubscription subscription)
    {
        _dbContext.UserSubscriptions.Add(subscription);
        await _dbContext.SaveChangesAsync();
    }

    public async Task UpdateAsync(UserSubscription subscription)
    {
        _dbContext.UserSubscriptions.Update(subscription);
        await _dbContext.SaveChangesAsync();
    }
}