using CodeImpact.Domain.Entities;

namespace CodeImpact.Domain.Repositories;

public interface IPlanRepository
{
    Task<IReadOnlyCollection<Plan>> ListActiveAsync();
    Task<Plan?> GetByIdAsync(Guid id);
    Task<Plan?> GetByNameAsync(string name);
    Task AddAsync(Plan plan);
}