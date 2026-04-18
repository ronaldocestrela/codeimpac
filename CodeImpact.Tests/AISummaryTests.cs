using CodeImpact.Application.AI;
using CodeImpact.Application.Common.Interfaces;
using CodeImpact.Domain.Entities;
using CodeImpact.Domain.Repositories;
using Microsoft.Extensions.Logging.Abstractions;

namespace CodeImpact.Tests;

public class AISummaryTests
{
    [Fact]
    public async Task AIOrchestrator_GenerateContributionSummaryAsync_UsesOnlyApprovedPullRequestsAndValidEvidenceIds()
    {
        var userId = Guid.NewGuid();
        var commit = new GitHubCommit(
            userId,
            Guid.NewGuid(),
            100,
            "org/repo",
            "abc123",
            "feat: add sync endpoint",
            "Dev One",
            "dev1@example.com",
            new DateTime(2026, 04, 10, 10, 0, 0, DateTimeKind.Utc),
            "https://github.com/org/repo/commit/abc123");

        var approvedPr = new GitHubPullRequest(
            userId,
            Guid.NewGuid(),
            100,
            "org/repo",
            501,
            12,
            "Improve contribution summary",
            "closed",
            "dev-one",
            true,
            new DateTime(2026, 04, 11, 12, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 04, 11, 16, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 04, 11, 16, 0, 0, DateTimeKind.Utc),
            "https://github.com/org/repo/pull/12");

        var notApprovedPr = new GitHubPullRequest(
            userId,
            Guid.NewGuid(),
            100,
            "org/repo",
            502,
            15,
            "Draft refactor",
            "open",
            "dev-two",
            false,
            new DateTime(2026, 04, 12, 9, 0, 0, DateTimeKind.Utc),
            null,
            null,
            "https://github.com/org/repo/pull/15");

        var orchestrator = new AIOrchestrator(
            new StubCommitRepository(new[] { commit }),
            new StubPullRequestRepository(new[] { approvedPr, notApprovedPr }),
            new ContributionPromptBuilder(),
            new StubLLMService("""
{
  "executiveSummary": "Entrega consistente com revisão aprovada.",
  "highlights": [
    {
      "title": "PR aprovado com impacto direto",
      "insight": "A melhoria foi concluída e validada.",
      "impact": "Aumenta previsibilidade para gestão.",
      "evidenceIds": ["APR-001", "APR-999"]
    }
  ]
}
"""),
            NullLogger<AIOrchestrator>.Instance);

        var result = await orchestrator.GenerateContributionSummaryAsync(new ContributionSummaryRequest(userId, null, null, null));

        Assert.Equal(1, result.Metrics.ApprovedPullRequestCount);
        Assert.Equal(1, result.Metrics.CommitCount);
        Assert.DoesNotContain(result.Evidence, e => e.ExternalReference == "15");
        Assert.Single(result.Highlights);
        var highlight = result.Highlights.First();
        Assert.Single(highlight.EvidenceIds);
        Assert.Equal("APR-001", highlight.EvidenceIds.Single());
    }

    [Fact]
    public void ContributionPromptBuilder_Build_AssignsDeterministicEvidenceIds()
    {
        var userId = Guid.NewGuid();
        var commits = new[]
        {
            new GitHubCommit(
                userId,
                Guid.NewGuid(),
                100,
                "org/repo",
                "bbb222",
                "feat: second",
                "Dev Two",
                "dev2@example.com",
                new DateTime(2026, 04, 11, 10, 0, 0, DateTimeKind.Utc),
                "https://github.com/org/repo/commit/bbb222"),
            new GitHubCommit(
                userId,
                Guid.NewGuid(),
                100,
                "org/repo",
                "aaa111",
                "feat: first",
                "Dev One",
                "dev1@example.com",
                new DateTime(2026, 04, 10, 10, 0, 0, DateTimeKind.Utc),
                "https://github.com/org/repo/commit/aaa111")
        };

        var approvedPrs = new[]
        {
            new GitHubPullRequest(
                userId,
                Guid.NewGuid(),
                100,
                "org/repo",
                501,
                20,
                "Add metrics",
                "closed",
                "dev-one",
                true,
                new DateTime(2026, 04, 12, 10, 0, 0, DateTimeKind.Utc),
                new DateTime(2026, 04, 12, 12, 0, 0, DateTimeKind.Utc),
                new DateTime(2026, 04, 12, 12, 0, 0, DateTimeKind.Utc),
                "https://github.com/org/repo/pull/20")
        };

        var prompt = new ContributionPromptBuilder().Build(new ContributionPromptInput(null, null, null, commits, approvedPrs));

        Assert.Equal(3, prompt.Evidence.Count);
        Assert.Equal("CMT-001", prompt.Evidence.ElementAt(0).EvidenceId);
        Assert.Equal("aaa111", prompt.Evidence.ElementAt(0).ExternalReference);
        Assert.Equal("CMT-002", prompt.Evidence.ElementAt(1).EvidenceId);
        Assert.Equal("bbb222", prompt.Evidence.ElementAt(1).ExternalReference);
        Assert.Equal("APR-001", prompt.Evidence.ElementAt(2).EvidenceId);
        Assert.Equal("20", prompt.Evidence.ElementAt(2).ExternalReference);
    }

    private sealed class StubLLMService : ILLMService
    {
        private readonly string _response;

        public StubLLMService(string response)
        {
            _response = response;
        }

        public Task<string> GenerateTextAsync(string systemPrompt, string userPrompt, CancellationToken cancellationToken = default)
            => Task.FromResult(_response);
    }

    private sealed class StubCommitRepository : IGitHubCommitRepository
    {
        private readonly IReadOnlyCollection<GitHubCommit> _items;

        public StubCommitRepository(IReadOnlyCollection<GitHubCommit> items)
        {
            _items = items;
        }

        public Task<GitHubCommit?> GetByUserRepositoryAndShaAsync(Guid userId, long repositoryId, string commitSha)
            => Task.FromResult<GitHubCommit?>(null);

        public Task<IReadOnlyCollection<GitHubCommit>> ListByUserAsync(Guid userId, long? repositoryId, DateTime? from, DateTime? to)
            => Task.FromResult(_items);

        public Task<GitHubCommit?> GetByIdAsync(Guid userId, Guid commitId)
            => Task.FromResult<GitHubCommit?>(null);

        public Task AddAsync(GitHubCommit commit) => Task.CompletedTask;

        public Task UpdateAsync(GitHubCommit commit) => Task.CompletedTask;
    }

    private sealed class StubPullRequestRepository : IGitHubPullRequestRepository
    {
        private readonly IReadOnlyCollection<GitHubPullRequest> _items;

        public StubPullRequestRepository(IReadOnlyCollection<GitHubPullRequest> items)
        {
            _items = items;
        }

        public Task<GitHubPullRequest?> GetByUserRepositoryAndGitHubPullRequestIdAsync(Guid userId, long repositoryId, long gitHubPullRequestId)
            => Task.FromResult<GitHubPullRequest?>(null);

        public Task<IReadOnlyCollection<GitHubPullRequest>> ListByUserAsync(Guid userId, long? repositoryId, DateTime? from, DateTime? to)
            => Task.FromResult(_items);

        public Task<GitHubPullRequest?> GetByIdAsync(Guid userId, Guid pullRequestId)
            => Task.FromResult<GitHubPullRequest?>(null);

        public Task AddAsync(GitHubPullRequest pullRequest) => Task.CompletedTask;

        public Task UpdateAsync(GitHubPullRequest pullRequest) => Task.CompletedTask;
    }
}