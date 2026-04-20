using CodeImpact.Application.Admin.Commands;
using CodeImpact.Application.Admin.Queries;
using CodeImpact.Application.Common.Interfaces;
using CodeImpact.Domain.Entities;
using CodeImpact.Domain.Repositories;

namespace CodeImpact.Tests;

public class AdminPhase10HandlersTests
{
    [Fact]
    public async Task UpdateAdminUserStatusCommandHandler_LogsSuccess()
    {
        var directory = new StubAdminUserDirectory { UpdateStatusResult = true };
        var auditRepository = new StubAdminAuditLogRepository();
        var handler = new UpdateAdminUserStatusCommandHandler(directory, auditRepository);

        var result = await handler.Handle(
            new UpdateAdminUserStatusCommand(Guid.NewGuid(), Guid.NewGuid(), "Suspended", "fraud-check", "127.0.0.1"),
            CancellationToken.None);

        Assert.True(result);
        Assert.Single(auditRepository.Items);
        Assert.Equal("UpdateUserStatus", auditRepository.Items[0].Action);
        Assert.Equal("success", auditRepository.Items[0].Result);
    }

    [Fact]
    public async Task RetryAdminJobCommandHandler_FailedJob_EnqueuesNewTask()
    {
        var userId = Guid.NewGuid();
        var failedJob = new BackgroundJobExecution(userId, BackgroundJobExecutionType.ExecutiveReport, "{}");
        failedJob.MarkFailed("boom");

        var jobRepository = new StubBackgroundJobExecutionRepository(failedJob);
        var scheduler = new StubBackgroundJobScheduler();
        var auditRepository = new StubAdminAuditLogRepository();
        var handler = new RetryAdminJobCommandHandler(jobRepository, scheduler, auditRepository);

        var newTaskId = await handler.Handle(new RetryAdminJobCommand(Guid.NewGuid(), failedJob.Id, "127.0.0.1"), CancellationToken.None);

        Assert.NotEqual(Guid.Empty, newTaskId);
        Assert.Contains(jobRepository.AddedJobs, job => job.Id == newTaskId);
        Assert.Single(auditRepository.Items, item => item.Action == "RetryJob" && item.Result == "success");
    }

    [Fact]
    public async Task GetAdminAuditLogsQueryHandler_ReturnsPagedResult()
    {
        var repository = new StubAdminAuditLogRepository();
        await repository.AddAsync(new AdminAuditLog(Guid.NewGuid(), "ForceUserResync", "User", Guid.NewGuid().ToString(), "repositoriesSynced=3", "success", null));
        var handler = new GetAdminAuditLogsQueryHandler(repository);

        var result = await handler.Handle(new GetAdminAuditLogsQuery(null, null, null, 1, 20), CancellationToken.None);

        Assert.Equal(1, result.TotalCount);
        Assert.Single(result.Items);
        Assert.Equal("ForceUserResync", result.Items.First().Action);
    }

    private sealed class StubAdminUserDirectory : IAdminUserDirectory
    {
        public bool UpdateStatusResult { get; set; }

        public Task<IReadOnlyCollection<AdminDirectoryUser>> ListUsersAsync(string? emailFilter, string? statusFilter, int page, int pageSize)
            => Task.FromResult((IReadOnlyCollection<AdminDirectoryUser>)Array.Empty<AdminDirectoryUser>());

        public Task<int> CountUsersAsync(string? emailFilter, string? statusFilter)
            => Task.FromResult(0);

        public Task<AdminDirectoryUser?> GetByIdAsync(Guid userId)
            => Task.FromResult<AdminDirectoryUser?>(null);

        public Task<bool> UpdateStatusAsync(Guid userId, string status)
            => Task.FromResult(UpdateStatusResult);

        public Task<bool> UpdateSupportFlagsAsync(Guid userId, string[] supportFlags)
            => Task.FromResult(true);
    }

    private sealed class StubAdminAuditLogRepository : IAdminAuditLogRepository
    {
        public List<AdminAuditLog> Items { get; } = new();

        public Task AddAsync(AdminAuditLog log)
        {
            Items.Add(log);
            return Task.CompletedTask;
        }

        public Task<IReadOnlyCollection<AdminAuditLog>> ListAsync(string? action, string? targetType, Guid? adminUserId, int page, int pageSize)
        {
            var query = Items.AsEnumerable();
            if (!string.IsNullOrWhiteSpace(action))
            {
                query = query.Where(item => item.Action == action);
            }

            if (!string.IsNullOrWhiteSpace(targetType))
            {
                query = query.Where(item => item.TargetType == targetType);
            }

            if (adminUserId.HasValue)
            {
                query = query.Where(item => item.AdminUserId == adminUserId.Value);
            }

            return Task.FromResult((IReadOnlyCollection<AdminAuditLog>)query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToArray());
        }

        public Task<int> CountAsync(string? action, string? targetType, Guid? adminUserId)
        {
            var query = Items.AsEnumerable();
            if (!string.IsNullOrWhiteSpace(action))
            {
                query = query.Where(item => item.Action == action);
            }

            if (!string.IsNullOrWhiteSpace(targetType))
            {
                query = query.Where(item => item.TargetType == targetType);
            }

            if (adminUserId.HasValue)
            {
                query = query.Where(item => item.AdminUserId == adminUserId.Value);
            }

            return Task.FromResult(query.Count());
        }
    }

    private sealed class StubBackgroundJobScheduler : IBackgroundJobScheduler
    {
        public string EnqueueContributionSummaryJob(Guid taskId) => $"job-contrib-{taskId}";
        public string EnqueueExecutiveReportJob(Guid taskId) => $"job-report-{taskId}";
    }

    private sealed class StubBackgroundJobExecutionRepository : IBackgroundJobExecutionRepository
    {
        private readonly BackgroundJobExecution _existing;
        public List<BackgroundJobExecution> AddedJobs { get; } = new();

        public StubBackgroundJobExecutionRepository(BackgroundJobExecution existing)
        {
            _existing = existing;
        }

        public Task AddAsync(BackgroundJobExecution execution)
        {
            AddedJobs.Add(execution);
            return Task.CompletedTask;
        }

        public Task<BackgroundJobExecution?> GetByIdAsync(Guid id)
        {
            if (_existing.Id == id)
            {
                return Task.FromResult<BackgroundJobExecution?>(_existing);
            }

            return Task.FromResult<BackgroundJobExecution?>(AddedJobs.FirstOrDefault(job => job.Id == id));
        }

        public Task<BackgroundJobExecution?> GetByIdForUserAsync(Guid userId, Guid id)
            => Task.FromResult<BackgroundJobExecution?>(null);

        public Task<IReadOnlyCollection<BackgroundJobExecution>> ListAsync(string? jobType, string? status, int page, int pageSize)
            => Task.FromResult((IReadOnlyCollection<BackgroundJobExecution>)Array.Empty<BackgroundJobExecution>());

        public Task<int> CountAsync(string? jobType, string? status)
            => Task.FromResult(0);

        public Task UpdateAsync(BackgroundJobExecution execution)
            => Task.CompletedTask;
    }
}