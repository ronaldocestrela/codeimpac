using System.Text.Json;
using CodeImpact.Application.Common.Interfaces;
using CodeImpact.Application.GitHub.Dto;
using CodeImpact.Domain.Entities;
using CodeImpact.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace CodeImpact.Application.BackgroundJobs;

public sealed class BackgroundGenerationJobProcessor
{
    private readonly IBackgroundJobExecutionRepository _jobRepository;
    private readonly IAIOrchestrator _aiOrchestrator;
    private readonly IExecutiveReportOrchestrator _reportOrchestrator;
    private readonly ILogger<BackgroundGenerationJobProcessor> _logger;

    public BackgroundGenerationJobProcessor(
        IBackgroundJobExecutionRepository jobRepository,
        IAIOrchestrator aiOrchestrator,
        IExecutiveReportOrchestrator reportOrchestrator,
        ILogger<BackgroundGenerationJobProcessor> logger)
    {
        _jobRepository = jobRepository;
        _aiOrchestrator = aiOrchestrator;
        _reportOrchestrator = reportOrchestrator;
        _logger = logger;
    }

    public async Task ProcessContributionSummaryAsync(Guid taskId)
    {
        var execution = await _jobRepository.GetByIdAsync(taskId);
        if (execution is null || execution.JobType != BackgroundJobExecutionType.ContributionSummary)
        {
            return;
        }

        try
        {
            execution.MarkProcessing();
            await _jobRepository.UpdateAsync(execution);

            var payload = JsonSerializer.Deserialize<ContributionSummaryJobRequest>(execution.RequestJson)
                          ?? throw new InvalidOperationException("Payload invalido para job de resumo de contribuicoes.");

            var summary = await _aiOrchestrator.GenerateContributionSummaryAsync(
                new ContributionSummaryRequest(execution.UserId, payload.RepositoryId, payload.From, payload.To));

            execution.MarkSucceeded(JsonSerializer.Serialize(summary));
            await _jobRepository.UpdateAsync(execution);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Falha ao processar job de resumo de contribuicoes. TaskId: {TaskId}", taskId);
            execution.MarkFailed(ex.Message);
            await _jobRepository.UpdateAsync(execution);
            throw;
        }
    }

    public async Task ProcessExecutiveReportAsync(Guid taskId)
    {
        var execution = await _jobRepository.GetByIdAsync(taskId);
        if (execution is null || execution.JobType != BackgroundJobExecutionType.ExecutiveReport)
        {
            return;
        }

        try
        {
            execution.MarkProcessing();
            await _jobRepository.UpdateAsync(execution);

            var payload = JsonSerializer.Deserialize<ExecutiveReportJobRequest>(execution.RequestJson)
                          ?? throw new InvalidOperationException("Payload invalido para job de relatorio executivo.");

            var report = await _reportOrchestrator.GenerateAndPersistAsync(
                new ExecutiveReportRequest(execution.UserId, payload.RepositoryId, payload.From, payload.To));

            execution.MarkSucceeded(JsonSerializer.Serialize(new ExecutiveReportJobResult(report.Id)));
            await _jobRepository.UpdateAsync(execution);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Falha ao processar job de relatorio executivo. TaskId: {TaskId}", taskId);
            execution.MarkFailed(ex.Message);
            await _jobRepository.UpdateAsync(execution);
            throw;
        }
    }
}

public sealed record ContributionSummaryJobRequest(long? RepositoryId, DateTime? From, DateTime? To);
public sealed record ExecutiveReportJobRequest(long? RepositoryId, DateTime? From, DateTime? To);
public sealed record ExecutiveReportJobResult(Guid ReportId);
