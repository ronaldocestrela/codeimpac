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
        var reports = await _reportRepository.ListByUserAsync(request.UserId, request.RepositoryId, request.OrganizationLogin, request.From, request.To);

        return reports
            .Select(report => new ExecutiveReportListItemDto(
                report.Id,
                report.GeneratedAt,
                report.RepositoryId,
                report.FromDate,
                report.ToDate,
                report.CommitCount,
                report.PullRequestApprovedCount,
                DeserializeRepositoriesCount(report),
                BuildPreview(report.ExecutiveSummary)))
            .ToList();
    }

    private static int DeserializeRepositoriesCount(CodeImpact.Domain.Entities.Report report)
    {
        try
        {
            var repositories = System.Text.Json.JsonSerializer.Deserialize<List<string>>(report.RepositoriesJson)
                ?? throw new InvalidOperationException("Lista de repositórios vazia na desserialização.");
            return repositories.Count;
        }
        catch (System.Text.Json.JsonException ex)
        {
            throw new InvalidOperationException($"Campo RepositoriesJson contém JSON inválido para o relatório {report.Id}.", ex);
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
