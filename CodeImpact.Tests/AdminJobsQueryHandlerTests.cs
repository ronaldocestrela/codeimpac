using CodeImpact.Application.Admin.Queries;
using CodeImpact.Domain.Entities;
using CodeImpact.Domain.Repositories;

namespace CodeImpact.Tests;

public class AdminJobsQueryHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsPagedJobsAndTotalCount()
    {
        var userId = Guid.NewGuid();
        var jobs = new List<BackgroundJobExecution>
        {
            new(userId, BackgroundJobExecutionType.ExecutiveReport, "{}"),
            new(userId, BackgroundJobExecutionType.ContributionSummary, "{}")
        };

        var repository = new StubBackgroundJobExecutionRepository(jobs);
        var handler = new GetAdminJobsQueryHandler(repository);

        var result = await handler.Handle(new GetAdminJobsQuery(null, null, 1, 10), CancellationToken.None);

        Assert.Equal(2, result.TotalCount);
        Assert.Equal(1, result.Page);
        Assert.Equal(10, result.PageSize);
        Assert.Equal(2, result.Items.Count);
    }

    [Fact]
    public async Task Handle_WithFilters_AppliesJobTypeAndStatus()
    {
        var userId = Guid.NewGuid();
        var matchingJob = new BackgroundJobExecution(userId, BackgroundJobExecutionType.ExecutiveReport, "{}");
        matchingJob.MarkFailed("error");

        var otherJob = new BackgroundJobExecution(userId, BackgroundJobExecutionType.ContributionSummary, "{}");
        var repository = new StubBackgroundJobExecutionRepository(new[] { matchingJob, otherJob });
        var handler = new GetAdminJobsQueryHandler(repository);

        var result = await handler.Handle(
            new GetAdminJobsQuery(BackgroundJobExecutionType.ExecutiveReport, BackgroundJobExecutionStatus.Failed, 1, 20),
            CancellationToken.None);

        Assert.Single(result.Items);
        Assert.Equal(BackgroundJobExecutionType.ExecutiveReport, result.Items.Single().JobType);
        Assert.Equal(BackgroundJobExecutionStatus.Failed, result.Items.Single().Status);
    }

    private sealed class StubBackgroundJobExecutionRepository : IBackgroundJobExecutionRepository
    {
        private readonly IReadOnlyCollection<BackgroundJobExecution> _jobs;

        public StubBackgroundJobExecutionRepository(IReadOnlyCollection<BackgroundJobExecution> jobs)
        {
            _jobs = jobs;
        }

        public Task AddAsync(BackgroundJobExecution execution) => Task.CompletedTask;

        public Task<BackgroundJobExecution?> GetByIdAsync(Guid id)
            => Task.FromResult(_jobs.FirstOrDefault(job => job.Id == id));

        public Task<BackgroundJobExecution?> GetByIdForUserAsync(Guid userId, Guid id)
            => Task.FromResult(_jobs.FirstOrDefault(job => job.UserId == userId && job.Id == id));

        public Task<IReadOnlyCollection<BackgroundJobExecution>> ListAsync(string? jobType, string? status, int page, int pageSize)
        {
            var query = _jobs.AsEnumerable();
            if (!string.IsNullOrWhiteSpace(jobType))
            {
                query = query.Where(job => job.JobType == jobType);
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(job => job.Status == status);
            }

            var items = query
                .OrderByDescending(job => job.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToArray();

            return Task.FromResult((IReadOnlyCollection<BackgroundJobExecution>)items);
        }

        public Task<int> CountAsync(string? jobType, string? status)
        {
            var query = _jobs.AsEnumerable();
            if (!string.IsNullOrWhiteSpace(jobType))
            {
                query = query.Where(job => job.JobType == jobType);
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(job => job.Status == status);
            }

            return Task.FromResult(query.Count());
        }

        public Task UpdateAsync(BackgroundJobExecution execution) => Task.CompletedTask;
    }
}