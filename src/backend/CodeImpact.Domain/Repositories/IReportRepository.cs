using CodeImpact.Domain.Entities;

namespace CodeImpact.Domain.Repositories;

public interface IReportRepository
{
    Task AddAsync(Report report);
    Task<Report?> GetByIdAsync(Guid userId, Guid reportId);
    Task<IReadOnlyCollection<Report>> ListByUserAsync(Guid userId, long? repositoryId, DateTime? from, DateTime? to);
}
