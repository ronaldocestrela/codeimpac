using CodeImpact.Domain.Entities;

namespace CodeImpact.Application.Common.Interfaces;

public interface IContributionPromptBuilder
{
    ContributionPrompt Build(ContributionPromptInput input);
}

public sealed record ContributionPromptInput(
    long? RepositoryId,
    DateTime? From,
    DateTime? To,
    IReadOnlyCollection<GitHubCommit> Commits,
    IReadOnlyCollection<GitHubPullRequest> ApprovedPullRequests);

public sealed record ContributionPrompt(
    string SystemPrompt,
    string UserPrompt,
    IReadOnlyCollection<ContributionPromptEvidence> Evidence,
    ContributionPromptMetrics Metrics);

public sealed record ContributionPromptEvidence(
    string EvidenceId,
    string EvidenceType,
    string RepositoryFullName,
    string ExternalReference,
    string Author,
    DateTime OccurredAt,
    string Status,
    string Url);

public sealed record ContributionPromptMetrics(
    int CommitCount,
    int ApprovedPullRequestCount,
    int RepositoryCount);