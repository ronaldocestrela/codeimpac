using CodeImpact.Application.Reports.Dto;

namespace CodeImpact.Application.Common.Interfaces;

public interface IExecutiveReportOrchestrator
{
    Task<ExecutiveReportDto> GenerateAndPersistAsync(ExecutiveReportRequest request, CancellationToken cancellationToken = default);
}

public sealed record ExecutiveReportRequest(
    Guid UserId,
    long? RepositoryId,
    DateTime? From,
    DateTime? To);
