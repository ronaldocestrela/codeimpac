using CodeImpact.Application.BackgroundJobs.Queries;
using CodeImpact.Domain.Entities;
using CodeImpact.Domain.Repositories;

namespace CodeImpact.Tests;

public class BackgroundJobStatusQueryHandlerTests
{
    [Fact]
    public async Task Handle_WithSucceededExecutiveReportAndCamelCaseReportId_ReturnsParsedReportId()
    {
        var userId = Guid.NewGuid();
        var expectedReportId = Guid.NewGuid();
        var execution = new BackgroundJobExecution(userId, BackgroundJobExecutionType.ExecutiveReport, "{}");
        execution.MarkSucceeded($"{{\"reportId\":\"{expectedReportId}\"}}");

        var handler = new GetBackgroundJobStatusQueryHandler(new StubBackgroundJobExecutionRepository(execution));

        var result = await handler.Handle(new GetBackgroundJobStatusQuery(userId, execution.Id), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(BackgroundJobExecutionStatus.Succeeded, result!.Status);
        Assert.Equal(expectedReportId, result.ReportId);
    }

    [Fact]
    public async Task Handle_WithSucceededExecutiveReportAndPascalCaseReportId_ReturnsParsedReportId()
    {
        var userId = Guid.NewGuid();
        var expectedReportId = Guid.NewGuid();
        var execution = new BackgroundJobExecution(userId, BackgroundJobExecutionType.ExecutiveReport, "{}");
        execution.MarkSucceeded($"{{\"ReportId\":\"{expectedReportId}\"}}");

        var handler = new GetBackgroundJobStatusQueryHandler(new StubBackgroundJobExecutionRepository(execution));

        var result = await handler.Handle(new GetBackgroundJobStatusQuery(userId, execution.Id), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(BackgroundJobExecutionStatus.Succeeded, result!.Status);
        Assert.Equal(expectedReportId, result.ReportId);
    }

    private sealed class StubBackgroundJobExecutionRepository : IBackgroundJobExecutionRepository
    {
        private readonly BackgroundJobExecution _execution;

        public StubBackgroundJobExecutionRepository(BackgroundJobExecution execution)
        {
            _execution = execution;
        }

        public Task AddAsync(BackgroundJobExecution execution) => Task.CompletedTask;

        public Task<BackgroundJobExecution?> GetByIdAsync(Guid id)
            => Task.FromResult(id == _execution.Id ? _execution : null);

        public Task<BackgroundJobExecution?> GetByIdForUserAsync(Guid userId, Guid id)
            => Task.FromResult(userId == _execution.UserId && id == _execution.Id ? _execution : null);

        public Task UpdateAsync(BackgroundJobExecution execution) => Task.CompletedTask;
    }
}
