using System.Text;
using CodeImpact.Application.Common.Interfaces;

namespace CodeImpact.Application.AI;

public sealed class ContributionPromptBuilder : IContributionPromptBuilder
{
    public ContributionPrompt Build(ContributionPromptInput input)
    {
        var orderedCommits = input.Commits
            .OrderBy(c => c.CommittedAt)
            .ThenBy(c => c.RepositoryFullName, StringComparer.OrdinalIgnoreCase)
            .ThenBy(c => c.CommitSha, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var orderedApprovedPrs = input.ApprovedPullRequests
            .OrderBy(pr => pr.CreatedAtGitHub)
            .ThenBy(pr => pr.RepositoryFullName, StringComparer.OrdinalIgnoreCase)
            .ThenBy(pr => pr.Number)
            .ToList();

        var evidence = new List<ContributionPromptEvidence>(orderedCommits.Count + orderedApprovedPrs.Count);

        for (var i = 0; i < orderedCommits.Count; i++)
        {
            var commit = orderedCommits[i];
            evidence.Add(new ContributionPromptEvidence(
                $"CMT-{i + 1:000}",
                "commit",
                commit.RepositoryFullName,
                commit.CommitSha,
                commit.AuthorName,
                commit.CommittedAt,
                "committed",
                commit.Url));
        }

        for (var i = 0; i < orderedApprovedPrs.Count; i++)
        {
            var pullRequest = orderedApprovedPrs[i];
            evidence.Add(new ContributionPromptEvidence(
                $"APR-{i + 1:000}",
                "approved_pull_request",
                pullRequest.RepositoryFullName,
                pullRequest.Number.ToString(),
                pullRequest.AuthorLogin,
                pullRequest.CreatedAtGitHub,
                "approved",
                pullRequest.Url));
        }

        var metrics = new ContributionPromptMetrics(
            orderedCommits.Count,
            orderedApprovedPrs.Count,
            evidence.Select(e => e.RepositoryFullName).Distinct(StringComparer.OrdinalIgnoreCase).Count());

        var userPrompt = BuildUserPrompt(input.RepositoryId, input.From, input.To, metrics, evidence);

        return new ContributionPrompt(
            BuildSystemPrompt(),
            userPrompt,
            evidence,
            metrics);
    }

    private static string BuildSystemPrompt()
    {
        return """
Você é um analista executivo de engenharia de software.
Sua saída deve ser objetiva, orientada a gestores e baseada estritamente nas evidências fornecidas.
Nunca invente fatos e nunca referencie IDs de evidência que não existam no input.

Responda SOMENTE em JSON válido com o formato:
{
  "executiveSummary": "string",
  "highlights": [
    {
      "title": "string",
      "insight": "string",
      "impact": "string",
      "evidenceIds": ["CMT-001", "APR-001"]
    }
  ]
}
""";
    }

    private static string BuildUserPrompt(
        long? repositoryId,
        DateTime? from,
        DateTime? to,
        ContributionPromptMetrics metrics,
        IReadOnlyCollection<ContributionPromptEvidence> evidence)
    {
        var sb = new StringBuilder();

        sb.AppendLine("CONTEXTO");
        sb.AppendLine($"- RepositoryId filtro: {(repositoryId.HasValue ? repositoryId.Value : "todos")}");
        sb.AppendLine($"- Período início: {(from.HasValue ? from.Value.ToString("O") : "não informado")}");
        sb.AppendLine($"- Período fim: {(to.HasValue ? to.Value.ToString("O") : "não informado")}");
        sb.AppendLine();
        sb.AppendLine("MÉTRICAS");
        sb.AppendLine($"- CommitCount: {metrics.CommitCount}");
        sb.AppendLine($"- ApprovedPullRequestCount: {metrics.ApprovedPullRequestCount}");
        sb.AppendLine($"- RepositoryCount: {metrics.RepositoryCount}");
        sb.AppendLine();
        sb.AppendLine("EVIDÊNCIAS");

        foreach (var item in evidence)
        {
            sb.AppendLine($"- [{item.EvidenceId}] type={item.EvidenceType}; repo={item.RepositoryFullName}; ref={item.ExternalReference}; author={item.Author}; occurredAt={item.OccurredAt:O}; status={item.Status}; url={item.Url}");
        }

        sb.AppendLine();
        sb.AppendLine("INSTRUÇÕES");
        sb.AppendLine("- Crie um resumo executivo curto para liderança técnica/gestão.");
        sb.AppendLine("- Gere até 5 highlights gerenciais com impacto e evidências rastreáveis.");
        sb.AppendLine("- Use apenas evidenceIds existentes na seção EVIDÊNCIAS.");

        return sb.ToString();
    }
}