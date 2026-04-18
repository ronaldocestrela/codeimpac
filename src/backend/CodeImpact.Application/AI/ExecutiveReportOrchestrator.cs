using System.Text;
using System.Text.Json;
using CodeImpact.Application.Common.Interfaces;
using CodeImpact.Application.Reports.Dto;
using CodeImpact.Domain.Entities;
using CodeImpact.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace CodeImpact.Application.AI;

public sealed class ExecutiveReportOrchestrator : IExecutiveReportOrchestrator
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly IGitHubCommitRepository _commitRepository;
    private readonly IGitHubPullRequestRepository _pullRequestRepository;
    private readonly IGitHubRepositorySelectionRepository _selectionRepository;
    private readonly IReportRepository _reportRepository;
    private readonly ILLMService _llmService;
    private readonly ILogger<ExecutiveReportOrchestrator> _logger;

    public ExecutiveReportOrchestrator(
        IGitHubCommitRepository commitRepository,
        IGitHubPullRequestRepository pullRequestRepository,
        IGitHubRepositorySelectionRepository selectionRepository,
        IReportRepository reportRepository,
        ILLMService llmService,
        ILogger<ExecutiveReportOrchestrator> logger)
    {
        _commitRepository = commitRepository;
        _pullRequestRepository = pullRequestRepository;
        _selectionRepository = selectionRepository;
        _reportRepository = reportRepository;
        _llmService = llmService;
        _logger = logger;
    }

    public async Task<ExecutiveReportDto> GenerateAndPersistAsync(ExecutiveReportRequest request, CancellationToken cancellationToken = default)
    {
        if (request.From.HasValue && request.To.HasValue && request.From > request.To)
        {
            throw new InvalidOperationException("Período inválido: 'from' deve ser menor ou igual a 'to'.");
        }

        var commits = await _commitRepository.ListByUserAsync(request.UserId, request.RepositoryId, request.From, request.To);
        var pullRequests = await _pullRequestRepository.ListByUserAsync(request.UserId, request.RepositoryId, request.From, request.To);
        var selectedRepositories = await _selectionRepository.GetByUserIdAsync(request.UserId);

        var repositories = BuildRepositoryScope(request.RepositoryId, selectedRepositories, commits, pullRequests);
        var evidence = BuildEvidence(commits, pullRequests);
        var metrics = BuildMetrics(commits, pullRequests, repositories.Count);
        var developerScope = request.UserId.ToString();

        var userPrompt = BuildUserPrompt(request, developerScope, repositories, metrics, evidence);
        var rawResponse = await _llmService.GenerateTextAsync(BuildSystemPrompt(), userPrompt, cancellationToken);
        var parsed = ParseResponse(rawResponse);
        var validEvidenceIds = evidence.Select(item => item.EvidenceId).ToHashSet(StringComparer.OrdinalIgnoreCase);

        var highlights = parsed.Highlights
            .Select(h => new ExecutiveReportHighlightDto(
                h.Title,
                h.Insight,
                h.Impact,
                h.EvidenceIds
                    .Where(id => validEvidenceIds.Contains(id))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList()))
            .Where(h => !string.IsNullOrWhiteSpace(h.Title) || !string.IsNullOrWhiteSpace(h.Insight))
            .ToList();

        var risks = parsed.Risks
            .Select(r => new ExecutiveReportRiskDto(
                r.Risk,
                r.Recommendation,
                r.EvidenceIds
                    .Where(id => validEvidenceIds.Contains(id))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList()))
            .Where(r => !string.IsNullOrWhiteSpace(r.Risk) || !string.IsNullOrWhiteSpace(r.Recommendation))
            .ToList();

        var generatedAt = DateTime.UtcNow;
        var report = new Report(
            request.UserId,
            request.RepositoryId,
            request.From,
            request.To,
            developerScope,
            JsonSerializer.Serialize(repositories, JsonOptions),
            metrics.CommitCount,
            metrics.PullRequestOpenCount,
            metrics.PullRequestClosedCount,
            metrics.PullRequestMergedCount,
            metrics.PullRequestApprovedCount,
            metrics.AverageMergeLeadTimeHours,
            parsed.ExecutiveSummary,
            JsonSerializer.Serialize(highlights, JsonOptions),
            JsonSerializer.Serialize(risks, JsonOptions),
            JsonSerializer.Serialize(evidence, JsonOptions),
            generatedAt);

        await _reportRepository.AddAsync(report);

        return new ExecutiveReportDto(
            report.Id,
            report.GeneratedAt,
            new ExecutiveReportScopeDto(
                report.DeveloperScope,
                report.RepositoryId,
                report.FromDate,
                report.ToDate,
                repositories),
            metrics,
            report.ExecutiveSummary,
            highlights,
            risks,
            evidence);
    }

    public static ExecutiveReportDto MapToDto(Report report)
    {
        var repositories = DeserializeOrDefault<List<string>>(report.RepositoriesJson) ?? new List<string>();
        var highlights = DeserializeOrDefault<List<ExecutiveReportHighlightDto>>(report.HighlightsJson) ?? new List<ExecutiveReportHighlightDto>();
        var risks = DeserializeOrDefault<List<ExecutiveReportRiskDto>>(report.RisksJson) ?? new List<ExecutiveReportRiskDto>();
        var evidence = DeserializeOrDefault<List<ExecutiveReportEvidenceDto>>(report.EvidenceJson) ?? new List<ExecutiveReportEvidenceDto>();

        return new ExecutiveReportDto(
            report.Id,
            report.GeneratedAt,
            new ExecutiveReportScopeDto(
                report.DeveloperScope,
                report.RepositoryId,
                report.FromDate,
                report.ToDate,
                repositories),
            new ExecutiveReportMetricsDto(
                report.CommitCount,
                report.PullRequestOpenCount,
                report.PullRequestClosedCount,
                report.PullRequestMergedCount,
                report.PullRequestApprovedCount,
                report.AverageMergeLeadTimeHours,
                repositories.Count),
            report.ExecutiveSummary,
            highlights,
            risks,
            evidence);
    }

    private static IReadOnlyCollection<string> BuildRepositoryScope(
        long? repositoryId,
        IReadOnlyCollection<GitHubRepositorySelection> selectedRepositories,
        IReadOnlyCollection<GitHubCommit> commits,
        IReadOnlyCollection<GitHubPullRequest> pullRequests)
    {
        if (repositoryId.HasValue)
        {
            var selected = selectedRepositories
                .FirstOrDefault(repo => repo.RepositoryId == repositoryId.Value)
                ?.FullName;

            if (!string.IsNullOrWhiteSpace(selected))
            {
                return new[] { selected };
            }
        }

        var fromContributions = commits
            .Select(c => c.RepositoryFullName)
            .Concat(pullRequests.Select(pr => pr.RepositoryFullName))
            .Where(name => !string.IsNullOrWhiteSpace(name));

        var fromSelection = repositoryId.HasValue
            ? selectedRepositories.Where(repo => repo.RepositoryId == repositoryId.Value).Select(repo => repo.FullName)
            : selectedRepositories.Select(repo => repo.FullName);

        return fromContributions
            .Concat(fromSelection)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static ExecutiveReportMetricsDto BuildMetrics(
        IReadOnlyCollection<GitHubCommit> commits,
        IReadOnlyCollection<GitHubPullRequest> pullRequests,
        int repositoryCount)
    {
        var mergedLeadTimesInHours = pullRequests
            .Where(pr => pr.MergedAtGitHub.HasValue)
            .Select(pr => (pr.MergedAtGitHub!.Value - pr.CreatedAtGitHub).TotalHours)
            .Where(hours => hours >= 0)
            .ToList();

        return new ExecutiveReportMetricsDto(
            commits.Count,
            pullRequests.Count(pr => string.Equals(pr.State, "open", StringComparison.OrdinalIgnoreCase)),
            pullRequests.Count(pr => string.Equals(pr.State, "closed", StringComparison.OrdinalIgnoreCase)),
            pullRequests.Count(pr => pr.MergedAtGitHub.HasValue),
            pullRequests.Count(pr => pr.IsApproved),
            mergedLeadTimesInHours.Count == 0 ? null : Math.Round(mergedLeadTimesInHours.Average(), 2),
            repositoryCount);
    }

    private static IReadOnlyCollection<ExecutiveReportEvidenceDto> BuildEvidence(
        IReadOnlyCollection<GitHubCommit> commits,
        IReadOnlyCollection<GitHubPullRequest> pullRequests)
    {
        var orderedCommits = commits
            .OrderBy(c => c.CommittedAt)
            .ThenBy(c => c.RepositoryFullName, StringComparer.OrdinalIgnoreCase)
            .ThenBy(c => c.CommitSha, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var orderedPullRequests = pullRequests
            .OrderBy(pr => pr.CreatedAtGitHub)
            .ThenBy(pr => pr.RepositoryFullName, StringComparer.OrdinalIgnoreCase)
            .ThenBy(pr => pr.Number)
            .ToList();

        var evidence = new List<ExecutiveReportEvidenceDto>(orderedCommits.Count + orderedPullRequests.Count);

        for (var index = 0; index < orderedCommits.Count; index++)
        {
            var commit = orderedCommits[index];
            evidence.Add(new ExecutiveReportEvidenceDto(
                $"CMT-{index + 1:000}",
                "commit",
                commit.RepositoryFullName,
                commit.CommitSha,
                commit.AuthorName,
                commit.CommittedAt,
                "committed",
                commit.Url));
        }

        for (var index = 0; index < orderedPullRequests.Count; index++)
        {
            var pullRequest = orderedPullRequests[index];
            evidence.Add(new ExecutiveReportEvidenceDto(
                $"PR-{index + 1:000}",
                "pull_request",
                pullRequest.RepositoryFullName,
                pullRequest.Number.ToString(),
                pullRequest.AuthorLogin,
                pullRequest.CreatedAtGitHub,
                BuildPullRequestStatus(pullRequest),
                pullRequest.Url));
        }

        return evidence;
    }

    private static string BuildPullRequestStatus(GitHubPullRequest pullRequest)
    {
        if (pullRequest.IsApproved)
        {
            return "approved";
        }

        if (pullRequest.MergedAtGitHub.HasValue)
        {
            return "merged";
        }

        return pullRequest.State;
    }

    private static string BuildSystemPrompt()
    {
        return """
Você é um analista executivo de engenharia focado em liderança.
Gere um relatório objetivo e orientado a ação, usando apenas evidências fornecidas.
Não invente fatos e não use IDs de evidência inexistentes.

Responda SOMENTE em JSON válido:
{
  "executiveSummary": "string",
  "highlights": [
    {
      "title": "string",
      "insight": "string",
      "impact": "string",
      "evidenceIds": ["CMT-001", "PR-001"]
    }
  ],
  "risks": [
    {
      "risk": "string",
      "recommendation": "string",
      "evidenceIds": ["PR-002"]
    }
  ]
}
""";
    }

    private static string BuildUserPrompt(
        ExecutiveReportRequest request,
        string developerScope,
        IReadOnlyCollection<string> repositories,
        ExecutiveReportMetricsDto metrics,
        IReadOnlyCollection<ExecutiveReportEvidenceDto> evidence)
    {
        var sb = new StringBuilder();
        sb.AppendLine("ESCOPO");
        sb.AppendLine($"- DeveloperScope: {developerScope}");
        sb.AppendLine($"- RepositoryId filtro: {(request.RepositoryId.HasValue ? request.RepositoryId.Value : "todos")}");
        sb.AppendLine($"- Repositórios analisados: {(repositories.Count == 0 ? "nenhum" : string.Join(", ", repositories))}");
        sb.AppendLine($"- Período início: {(request.From.HasValue ? request.From.Value.ToString("O") : "não informado")}");
        sb.AppendLine($"- Período fim: {(request.To.HasValue ? request.To.Value.ToString("O") : "não informado")}");
        sb.AppendLine();

        sb.AppendLine("KPIs");
        sb.AppendLine($"- CommitCount: {metrics.CommitCount}");
        sb.AppendLine($"- PullRequestOpenCount: {metrics.PullRequestOpenCount}");
        sb.AppendLine($"- PullRequestClosedCount: {metrics.PullRequestClosedCount}");
        sb.AppendLine($"- PullRequestMergedCount: {metrics.PullRequestMergedCount}");
        sb.AppendLine($"- PullRequestApprovedCount: {metrics.PullRequestApprovedCount}");
        sb.AppendLine($"- AverageMergeLeadTimeHours: {(metrics.AverageMergeLeadTimeHours.HasValue ? metrics.AverageMergeLeadTimeHours.Value.ToString("0.##") : "n/a")}");
        sb.AppendLine();

        sb.AppendLine("EVIDÊNCIAS");
        foreach (var item in evidence)
        {
            sb.AppendLine($"- [{item.EvidenceId}] type={item.EvidenceType}; repo={item.RepositoryFullName}; ref={item.ExternalReference}; author={item.Author}; occurredAt={item.OccurredAt:O}; status={item.Status}; url={item.Url}");
        }

        sb.AppendLine();
        sb.AppendLine("INSTRUÇÕES");
        sb.AppendLine("- Crie resumo executivo curto para gestores/supervisores.");
        sb.AppendLine("- Traga até 5 highlights com impacto de negócio e IDs de evidência.");
        sb.AppendLine("- Traga até 3 riscos/itens bloqueadores com recomendação objetiva e IDs de evidência.");

        return sb.ToString();
    }

    private ParsedReportResponse ParseResponse(string rawResponse)
    {
        try
        {
            using var document = JsonDocument.Parse(rawResponse);
            var root = document.RootElement;

            var executiveSummary = root.TryGetProperty("executiveSummary", out var summaryElement)
                ? summaryElement.GetString() ?? string.Empty
                : string.Empty;

            var highlights = new List<ParsedHighlight>();
            if (root.TryGetProperty("highlights", out var highlightsElement)
                && highlightsElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in highlightsElement.EnumerateArray())
                {
                    highlights.Add(new ParsedHighlight(
                        item.TryGetProperty("title", out var titleElement) ? titleElement.GetString() ?? string.Empty : string.Empty,
                        item.TryGetProperty("insight", out var insightElement) ? insightElement.GetString() ?? string.Empty : string.Empty,
                        item.TryGetProperty("impact", out var impactElement) ? impactElement.GetString() ?? string.Empty : string.Empty,
                        ReadEvidenceIds(item)));
                }
            }

            var risks = new List<ParsedRisk>();
            if (root.TryGetProperty("risks", out var risksElement)
                && risksElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in risksElement.EnumerateArray())
                {
                    risks.Add(new ParsedRisk(
                        item.TryGetProperty("risk", out var riskElement) ? riskElement.GetString() ?? string.Empty : string.Empty,
                        item.TryGetProperty("recommendation", out var recommendationElement) ? recommendationElement.GetString() ?? string.Empty : string.Empty,
                        ReadEvidenceIds(item)));
                }
            }

            return new ParsedReportResponse(executiveSummary, highlights, risks);
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Resposta do LLM para relatório executivo não está em JSON válido. Será aplicado fallback textual.");
            return new ParsedReportResponse(rawResponse.Trim(), Array.Empty<ParsedHighlight>(), Array.Empty<ParsedRisk>());
        }
    }

    private static IReadOnlyCollection<string> ReadEvidenceIds(JsonElement item)
    {
        if (!item.TryGetProperty("evidenceIds", out var evidenceElement) || evidenceElement.ValueKind != JsonValueKind.Array)
        {
            return Array.Empty<string>();
        }

        return evidenceElement
            .EnumerateArray()
            .Where(value => value.ValueKind == JsonValueKind.String)
            .Select(value => value.GetString() ?? string.Empty)
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .ToList();
    }

    private static T? DeserializeOrDefault<T>(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<T>(json, JsonOptions);
        }
        catch (JsonException)
        {
            return default;
        }
    }

    private sealed record ParsedReportResponse(
        string ExecutiveSummary,
        IReadOnlyCollection<ParsedHighlight> Highlights,
        IReadOnlyCollection<ParsedRisk> Risks);

    private sealed record ParsedHighlight(
        string Title,
        string Insight,
        string Impact,
        IReadOnlyCollection<string> EvidenceIds);

    private sealed record ParsedRisk(
        string Risk,
        string Recommendation,
        IReadOnlyCollection<string> EvidenceIds);
}
