using CodeImpact.Domain.Entities;
using CodeImpact.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CodeImpact.Tests;

public class ReportRepositoryTests
{
    [Fact]
    public async Task ListByUserAsync_FiltersByAnalyzedPeriod_NotByGeneratedAt()
    {
        var userId = Guid.NewGuid();

        await using var dbContext = BuildDbContext();

        var reportInScope = CreateReport(
            userId,
            fromDate: new DateTime(2026, 04, 01, 0, 0, 0, DateTimeKind.Utc),
            toDate: new DateTime(2026, 04, 10, 23, 59, 59, DateTimeKind.Utc),
            generatedAt: new DateTime(2026, 01, 01, 0, 0, 0, DateTimeKind.Utc));

        var reportOutOfScope = CreateReport(
            userId,
            fromDate: new DateTime(2026, 03, 01, 0, 0, 0, DateTimeKind.Utc),
            toDate: new DateTime(2026, 03, 10, 23, 59, 59, DateTimeKind.Utc),
            generatedAt: new DateTime(2026, 04, 06, 0, 0, 0, DateTimeKind.Utc));

        dbContext.Reports.Add(reportInScope);
        dbContext.Reports.Add(reportOutOfScope);
        await dbContext.SaveChangesAsync();

        var repository = new ReportRepository(dbContext);

        var result = await repository.ListByUserAsync(
            userId,
            repositoryId: null,
            organizationLogin: null,
            from: new DateTime(2026, 04, 05, 0, 0, 0, DateTimeKind.Utc),
            to: new DateTime(2026, 04, 20, 0, 0, 0, DateTimeKind.Utc));

        var report = Assert.Single(result);
        Assert.Equal(reportInScope.Id, report.Id);
    }

    private static CodeImpactDbContext BuildDbContext()
    {
        var options = new DbContextOptionsBuilder<CodeImpactDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new CodeImpactDbContext(options);
    }

    private static Report CreateReport(Guid userId, DateTime fromDate, DateTime toDate, DateTime generatedAt)
    {
        return new Report(
            userId,
            repositoryId: 100,
            fromDate,
            toDate,
            developerScope: userId.ToString(),
            repositoriesJson: "[\"org/repo\"]",
            commitCount: 2,
            pullRequestOpenCount: 1,
            pullRequestClosedCount: 1,
            pullRequestMergedCount: 1,
            pullRequestApprovedCount: 1,
            averageMergeLeadTimeHours: 4,
            executiveSummary: "Resumo",
            highlightsJson: "[]",
            risksJson: "[]",
            evidenceJson: "[]",
            generatedAt);
    }
}
