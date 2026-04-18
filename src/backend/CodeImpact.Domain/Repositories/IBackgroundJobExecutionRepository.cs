using CodeImpact.Domain.Entities;

namespace CodeImpact.Domain.Repositories;

public interface IBackgroundJobExecutionRepository
{
    Task AddAsync(BackgroundJobExecution execution);
    Task<BackgroundJobExecution?> GetByIdAsync(Guid id);
    Task<BackgroundJobExecution?> GetByIdForUserAsync(Guid userId, Guid id);
    Task UpdateAsync(BackgroundJobExecution execution);
}
