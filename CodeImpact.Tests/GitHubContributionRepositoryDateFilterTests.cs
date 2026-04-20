using CodeImpact.Domain.Entities;
using CodeImpact.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CodeImpact.Tests;

public class GitHubContributionRepositoryDateFilterTests
{
    [Fact]
    public async Task CommitRepository_ListByUserAsync_IncludesItemsOnEndDate()
    {
        var userId = Guid.NewGuid();

        await using var dbContext = BuildDbContext();

        var includedCommit = new GitHubCommit(
            userId,
            Guid.NewGuid(),
            100,
            "octocat/repo",
            "sha-included",
            "included",
            "Octo Cat",
            "octo@example.com",
            new DateTime(2026, 4, 10, 14, 30, 0, DateTimeKind.Utc),
            "https://github.com/octocat/repo/commit/sha-included");

        var excludedCommit = new GitHubCommit(
            userId,
            Guid.NewGuid(),
            100,
            "octocat/repo",
            "sha-excluded",
            "excluded",
            "Octo Cat",
            "octo@example.com",
            new DateTime(2026, 4, 11, 0, 0, 0, DateTimeKind.Utc),
            "https://github.com/octocat/repo/commit/sha-excluded");

        dbContext.GitHubCommits.Add(includedCommit);
        dbContext.GitHubCommits.Add(excludedCommit);
        await dbContext.SaveChangesAsync();

        var repository = new GitHubCommitRepository(dbContext);

        var result = await repository.ListByUserAsync(
            userId,
            repositoryId: 100,
            organizationLogin: null,
            from: null,
            to: new DateTime(2026, 4, 10, 0, 0, 0, DateTimeKind.Utc));

        var commit = Assert.Single(result);
        Assert.Equal(includedCommit.Id, commit.Id);
    }

    [Fact]
    public async Task PullRequestRepository_ListByUserAsync_IncludesItemsOnEndDate()
    {
        var userId = Guid.NewGuid();

        await using var dbContext = BuildDbContext();

        var includedPullRequest = new GitHubPullRequest(
            userId,
            Guid.NewGuid(),
            100,
            "octocat/repo",
            1001,
            42,
            "included",
            "open",
            "octocat",
            true,
            new DateTime(2026, 4, 10, 18, 0, 0, DateTimeKind.Utc),
            null,
            null,
            "https://github.com/octocat/repo/pull/42");

        var excludedPullRequest = new GitHubPullRequest(
            userId,
            Guid.NewGuid(),
            100,
            "octocat/repo",
            1002,
            43,
            "excluded",
            "open",
            "octocat",
            false,
            new DateTime(2026, 4, 11, 0, 0, 0, DateTimeKind.Utc),
            null,
            null,
            "https://github.com/octocat/repo/pull/43");

        dbContext.GitHubPullRequests.Add(includedPullRequest);
        dbContext.GitHubPullRequests.Add(excludedPullRequest);
        await dbContext.SaveChangesAsync();

        var repository = new GitHubPullRequestRepository(dbContext);

        var result = await repository.ListByUserAsync(
            userId,
            repositoryId: 100,
            organizationLogin: null,
            from: null,
            to: new DateTime(2026, 4, 10, 0, 0, 0, DateTimeKind.Utc));

        var pullRequest = Assert.Single(result);
        Assert.Equal(includedPullRequest.Id, pullRequest.Id);
    }

    private static CodeImpactDbContext BuildDbContext()
    {
        var options = new DbContextOptionsBuilder<CodeImpactDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new CodeImpactDbContext(options);
    }
}
