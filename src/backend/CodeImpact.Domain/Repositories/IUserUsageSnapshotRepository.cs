using CodeImpact.Domain.Entities;

namespace CodeImpact.Domain.Repositories;

public interface IUserUsageSnapshotRepository
{
    Task<UserUsageSnapshot?> GetByUserIdAsync(Guid userId);
    Task AddAsync(UserUsageSnapshot snapshot);
    Task UpdateAsync(UserUsageSnapshot snapshot);
}