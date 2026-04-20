using CodeImpact.Application.AI;
using CodeImpact.Application.Common.Interfaces;
using CodeImpact.Application.Reports.Queries;
using CodeImpact.Domain.Entities;
using CodeImpact.Domain.Repositories;
using Microsoft.Extensions.Logging.Abstractions;

namespace CodeImpact.Tests;

public class ReportGenerationTests
{
    [Fact]
    public async Task ExecutiveReportOrchestrator_GenerateAndPersistAsync_PersistsMetricsNarrativeAndEvidence()
    {
        var userId = Guid.NewGuid();
        var commits = new[]
        {
            new GitHubCommit(
                userId,
                Guid.NewGuid(),
                100,
                "org/repo",
                "abc123",
                "feat: add export",
                "Dev One",
                "dev1@example.com",
                new DateTime(2026, 04, 12, 10, 0, 0, DateTimeKind.Utc),
                "https://github.com/org/repo/commit/abc123")
        };

        var pullRequests = new[]
        {
            new GitHubPullRequest(
                userId,
                Guid.NewGuid(),
                100,
                "org/repo",
                501,
                12,
                "Add export section",
                "closed",
                "dev-one",
                true,
                new DateTime(2026, 04, 10, 10, 0, 0, DateTimeKind.Utc),
                new DateTime(2026, 04, 12, 12, 0, 0, DateTimeKind.Utc),
                new DateTime(2026, 04, 12, 12, 0, 0, DateTimeKind.Utc),
                "https://github.com/org/repo/pull/12")
        };

        var reportRepository = new StubReportRepository();
        var orchestrator = new ExecutiveReportOrchestrator(
            new StubCommitRepository(commits),
            new StubPullRequestRepository(pullRequests),
            new StubSelectionRepository(new[]
            {
                new GitHubRepositorySelection(userId, Guid.NewGuid(), 100, "repo", "org/repo", false, "org", "Organization")
            }),
            reportRepository,
            new StubLLMService("""
{
  "executiveSummary": "Entrega consistente e previsível no período.",
  "highlights": [
    {
      "title": "PR aprovado com evidência rastreável",
      "insight": "Houve revisão e fechamento da entrega.",
      "impact": "Reduz risco de retrabalho.",
      "evidenceIds": ["PR-001", "PR-999"]
    }
  ],
  "risks": [
    {
      "risk": "Dependência de revisão externa",
      "recommendation": "Definir SLA de revisão entre times.",
      "evidenceIds": ["PR-001"]
    }
  ]
}
"""),
            NullLogger<ExecutiveReportOrchestrator>.Instance);

        var result = await orchestrator.GenerateAndPersistAsync(new ExecutiveReportRequest(userId, null, null, null, null));

        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal(1, result.Metrics.CommitCount);
        Assert.Equal(1, result.Metrics.PullRequestApprovedCount);
        Assert.Equal(50, result.Metrics.AverageMergeLeadTimeHours);
        Assert.Single(result.Highlights);
        var highlight = result.Highlights.Single();
        Assert.Single(highlight.EvidenceIds);
        Assert.Equal("PR-001", highlight.EvidenceIds.Single());
        Assert.Single(result.Risks);

        Assert.NotNull(reportRepository.AddedReport);
        Assert.Equal(result.Id, reportRepository.AddedReport!.Id);
        Assert.Contains("org/repo", reportRepository.AddedReport.RepositoriesJson);
    }

    [Fact]
    public async Task GetExecutiveReportsQueryHandler_ReturnsListWithSummaryPreview()
    {
        var userId = Guid.NewGuid();
        var now = DateTime.UtcNow;

        var repository = new StubReportRepository
        {
            ListResult = new[]
            {
                new Report(
                    userId,
                    null,
                    now.AddDays(-7),
                    now,
                    userId.ToString(),
                    "[\"org/repo\",\"org/infra\"]",
                    5,
                    1,
                    2,
                    1,
                    1,
                    24,
                    "Resumo executivo muito grande para validar preview truncado e manter legibilidade para listagem de liderança.",
                    "[]",
                    "[]",
                    "[]",
                    now)
            }
        };

        var handler = new GetExecutiveReportsQueryHandler(repository);
        var result = await handler.Handle(new GetExecutiveReportsQuery(userId, null, null, null, null), CancellationToken.None);

        Assert.Single(result);
        var item = result.Single();
        Assert.Equal(2, item.RepositoryCount);
        Assert.True(item.ExecutiveSummaryPreview.Length <= 160);
    }

    [Fact]
    public async Task ExecutiveReportOrchestrator_GenerateAndPersistAsync_WithInvalidLlmJson_ThrowsAndDoesNotPersist()
    {
        var userId = Guid.NewGuid();
        var reportRepository = new StubReportRepository();
        var orchestrator = new ExecutiveReportOrchestrator(
            new StubCommitRepository(Array.Empty<GitHubCommit>()),
            new StubPullRequestRepository(Array.Empty<GitHubPullRequest>()),
            new StubSelectionRepository(Array.Empty<GitHubRepositorySelection>()),
            reportRepository,
            new StubLLMService("resposta sem json valido"),
            NullLogger<ExecutiveReportOrchestrator>.Instance);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            orchestrator.GenerateAndPersistAsync(new ExecutiveReportRequest(userId, null, null, null, null)));

        Assert.Null(reportRepository.AddedReport);
    }

    [Fact]
    public async Task GetExecutiveReportsQueryHandler_WithCorruptedRepositoriesJson_ThrowsInvalidOperationException()
    {
        var userId = Guid.NewGuid();
        var now = DateTime.UtcNow;

        var report = new Report(
            userId,
            null,
            now.AddDays(-1),
            now,
            userId.ToString(),
            "[]",
            1,
            0,
            1,
            0,
            0,
            null,
            "Resumo",
            "[]",
            "[]",
            "[]",
            now);

        var repositoriesProperty = typeof(Report).GetProperty(nameof(Report.RepositoriesJson));
        repositoriesProperty!.SetValue(report, "[invalid-json");

        var repository = new StubReportRepository
        {
            ListResult = new[] { report }
        };

        var handler = new GetExecutiveReportsQueryHandler(repository);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.Handle(new GetExecutiveReportsQuery(userId, null, null, null, null), CancellationToken.None));
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

        public Task<IReadOnlyCollection<GitHubCommit>> ListByUserAsync(Guid userId, long? repositoryId, string? organizationLogin, DateTime? from, DateTime? to)
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

        public Task<IReadOnlyCollection<GitHubPullRequest>> ListByUserAsync(Guid userId, long? repositoryId, string? organizationLogin, DateTime? from, DateTime? to)
            => Task.FromResult(_items);

        public Task<GitHubPullRequest?> GetByIdAsync(Guid userId, Guid pullRequestId)
            => Task.FromResult<GitHubPullRequest?>(null);

        public Task AddAsync(GitHubPullRequest pullRequest) => Task.CompletedTask;

        public Task UpdateAsync(GitHubPullRequest pullRequest) => Task.CompletedTask;
    }

    private sealed class StubSelectionRepository : IGitHubRepositorySelectionRepository
    {
        private readonly IReadOnlyCollection<GitHubRepositorySelection> _items;

        public StubSelectionRepository(IReadOnlyCollection<GitHubRepositorySelection> items)
        {
            _items = items;
        }

        public Task<IReadOnlyCollection<GitHubRepositorySelection>> GetByUserIdAsync(Guid userId)
            => Task.FromResult(_items);

        public Task<GitHubRepositorySelection?> GetByUserAndRepositoryIdAsync(Guid userId, long repositoryId)
            => Task.FromResult(_items.FirstOrDefault(item => item.RepositoryId == repositoryId));

        public Task ReplaceForUserAsync(Guid userId, Guid gitHubAccountId, IEnumerable<GitHubRepositorySelection> selections, string? ownerLoginScope = null)
            => Task.CompletedTask;
    }

    private sealed class StubReportRepository : IReportRepository
    {
        public Report? AddedReport { get; private set; }

        public IReadOnlyCollection<Report> ListResult { get; set; } = Array.Empty<Report>();

        public Task AddAsync(Report report)
        {
            AddedReport = report;
            return Task.CompletedTask;
        }

        public Task<Report?> GetByIdAsync(Guid userId, Guid reportId)
            => Task.FromResult(ListResult.FirstOrDefault(r => r.UserId == userId && r.Id == reportId) ?? AddedReport);

        public Task<IReadOnlyCollection<Report>> ListByUserAsync(Guid userId, long? repositoryId, string? organizationLogin, DateTime? from, DateTime? to)
            => Task.FromResult(ListResult);
    }
}
