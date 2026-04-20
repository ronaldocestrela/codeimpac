using CodeImpact.Domain.Entities;

namespace CodeImpact.Domain.Repositories;

public interface IBackgroundJobExecutionRepository
{
    Task AddAsync(BackgroundJobExecution execution);
    Task<BackgroundJobExecution?> GetByIdAsync(Guid id);
    Task<BackgroundJobExecution?> GetByIdForUserAsync(Guid userId, Guid id);
    Task<IReadOnlyCollection<BackgroundJobExecution>> ListAsync(string? jobType, string? status, int page, int pageSize);
    Task<int> CountAsync(string? jobType, string? status);
    Task UpdateAsync(BackgroundJobExecution execution);
}
