using CodeImpact.Domain.Entities;

namespace CodeImpact.Domain.Repositories;

public interface IUserSubscriptionRepository
{
    Task<UserSubscription?> GetByUserIdAsync(Guid userId);
    Task AddAsync(UserSubscription subscription);
    Task UpdateAsync(UserSubscription subscription);
}