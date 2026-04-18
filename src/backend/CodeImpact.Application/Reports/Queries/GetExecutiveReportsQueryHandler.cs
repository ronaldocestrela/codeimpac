using CodeImpact.Application.Reports.Dto;
using CodeImpact.Domain.Repositories;
using MediatR;

namespace CodeImpact.Application.Reports.Queries;

public sealed class GetExecutiveReportsQueryHandler : IRequestHandler<GetExecutiveReportsQuery, IReadOnlyCollection<ExecutiveReportListItemDto>>
{
    private readonly IReportRepository _reportRepository;

    public GetExecutiveReportsQueryHandler(IReportRepository reportRepository)
    {
        _reportRepository = reportRepository;
    }

    public async Task<IReadOnlyCollection<ExecutiveReportListItemDto>> Handle(GetExecutiveReportsQuery request, CancellationToken cancellationToken)
    {
        var reports = await _reportRepository.ListByUserAsync(request.UserId, request.RepositoryId, request.From, request.To);

        return reports
            .Select(report => new ExecutiveReportListItemDto(
                report.Id,
                report.GeneratedAt,
                report.RepositoryId,
                report.FromDate,
                report.ToDate,
                report.CommitCount,
                report.PullRequestApprovedCount,
                DeserializeRepositoriesCount(report.RepositoriesJson),
                BuildPreview(report.ExecutiveSummary)))
            .ToList();
    }

    private static int DeserializeRepositoriesCount(string repositoriesJson)
    {
        try
        {
            var repositories = System.Text.Json.JsonSerializer.Deserialize<List<string>>(repositoriesJson);
            return repositories?.Count ?? 0;
        }
        catch (System.Text.Json.JsonException)
        {
            return 0;
        }
    }

    private static string BuildPreview(string summary)
    {
        if (string.IsNullOrWhiteSpace(summary))
        {
            return string.Empty;
        }

        return summary.Length <= 160 ? summary : $"{summary[..157]}...";
    }
}
