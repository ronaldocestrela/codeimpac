using System.Text.Json;
using CodeImpact.Application.Common.Interfaces;
using CodeImpact.Application.GitHub.Dto;
using CodeImpact.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace CodeImpact.Application.AI;

public sealed class AIOrchestrator : IAIOrchestrator
{
    private readonly IGitHubCommitRepository _commitRepository;
    private readonly IGitHubPullRequestRepository _pullRequestRepository;
    private readonly IContributionPromptBuilder _promptBuilder;
    private readonly ILLMService _llmService;
    private readonly ILogger<AIOrchestrator> _logger;

    public AIOrchestrator(
        IGitHubCommitRepository commitRepository,
        IGitHubPullRequestRepository pullRequestRepository,
        IContributionPromptBuilder promptBuilder,
        ILLMService llmService,
        ILogger<AIOrchestrator> logger)
    {
        _commitRepository = commitRepository;
        _pullRequestRepository = pullRequestRepository;
        _promptBuilder = promptBuilder;
        _llmService = llmService;
        _logger = logger;
    }

    public async Task<ContributionSummaryDto> GenerateContributionSummaryAsync(ContributionSummaryRequest request, CancellationToken cancellationToken = default)
    {
        if (request.From.HasValue && request.To.HasValue && request.From > request.To)
        {
            throw new InvalidOperationException("Período inválido: 'from' deve ser menor ou igual a 'to'.");
        }

        var commits = await _commitRepository.ListByUserAsync(request.UserId, request.RepositoryId, request.From, request.To);
        var pullRequests = await _pullRequestRepository.ListByUserAsync(request.UserId, request.RepositoryId, request.From, request.To);

        var approvedPullRequests = pullRequests
            .Where(pr => pr.IsApproved)
            .ToList();

        var prompt = _promptBuilder.Build(new ContributionPromptInput(
            request.RepositoryId,
            request.From,
            request.To,
            commits,
            approvedPullRequests));

        var rawResponse = await _llmService.GenerateTextAsync(prompt.SystemPrompt, prompt.UserPrompt, cancellationToken);
        var parsed = ParseResponse(rawResponse);
        var validEvidenceIds = prompt.Evidence.Select(e => e.EvidenceId).ToHashSet(StringComparer.OrdinalIgnoreCase);

        var highlights = parsed.Highlights
            .Select(highlight => new ContributionHighlightDto(
                highlight.Title,
                highlight.Insight,
                highlight.Impact,
                highlight.EvidenceIds
                    .Where(id => validEvidenceIds.Contains(id))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList()))
            .Where(highlight => !string.IsNullOrWhiteSpace(highlight.Title) || !string.IsNullOrWhiteSpace(highlight.Insight))
            .ToList();

        return new ContributionSummaryDto(
            DateTime.UtcNow,
            new ContributionSummaryScopeDto(request.RepositoryId, request.From, request.To),
            new ContributionSummaryMetricsDto(
                prompt.Metrics.CommitCount,
                prompt.Metrics.ApprovedPullRequestCount,
                prompt.Metrics.RepositoryCount),
            parsed.ExecutiveSummary,
            highlights,
            prompt.Evidence
                .Select(e => new ContributionSummaryEvidenceDto(
                    e.EvidenceId,
                    e.EvidenceType,
                    e.RepositoryFullName,
                    e.ExternalReference,
                    e.Author,
                    e.OccurredAt,
                    e.Status,
                    e.Url))
                .ToList());
    }

    private ParsedLLMResponse ParseResponse(string rawResponse)
    {
        if (string.IsNullOrWhiteSpace(rawResponse))
        {
            throw new InvalidOperationException("Resposta do LLM para resumo de contribuições está vazia.");
        }

        try
        {
            using var document = JsonDocument.Parse(rawResponse);
            var root = document.RootElement;

            var executiveSummary = root.TryGetProperty("executiveSummary", out var summaryElement)
                ? summaryElement.GetString() ?? string.Empty
                : string.Empty;

            var highlights = new List<ParsedHighlight>();
            if (root.TryGetProperty("highlights", out var highlightsElement) && highlightsElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var highlightElement in highlightsElement.EnumerateArray())
                {
                    var evidenceIds = new List<string>();
                    if (highlightElement.TryGetProperty("evidenceIds", out var evidenceIdsElement)
                        && evidenceIdsElement.ValueKind == JsonValueKind.Array)
                    {
                        evidenceIds.AddRange(evidenceIdsElement
                            .EnumerateArray()
                            .Where(item => item.ValueKind == JsonValueKind.String)
                            .Select(item => item.GetString() ?? string.Empty)
                            .Where(value => !string.IsNullOrWhiteSpace(value)));
                    }

                    highlights.Add(new ParsedHighlight(
                        highlightElement.TryGetProperty("title", out var titleElement) ? titleElement.GetString() ?? string.Empty : string.Empty,
                        highlightElement.TryGetProperty("insight", out var insightElement) ? insightElement.GetString() ?? string.Empty : string.Empty,
                        highlightElement.TryGetProperty("impact", out var impactElement) ? impactElement.GetString() ?? string.Empty : string.Empty,
                        evidenceIds));
                }
            }

            return new ParsedLLMResponse(executiveSummary, highlights);
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Resposta do LLM para resumo de contribuições não está em JSON válido.");
            throw new InvalidOperationException("Resposta do LLM para resumo de contribuições não está em JSON válido.", ex);
        }
    }

    private sealed record ParsedLLMResponse(string ExecutiveSummary, IReadOnlyCollection<ParsedHighlight> Highlights);

    private sealed record ParsedHighlight(string Title, string Insight, string Impact, IReadOnlyCollection<string> EvidenceIds);
}