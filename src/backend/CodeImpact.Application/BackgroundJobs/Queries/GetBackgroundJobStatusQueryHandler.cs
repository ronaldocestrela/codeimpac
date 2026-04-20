using System.Text.Json;
using CodeImpact.Application.BackgroundJobs.Dto;
using CodeImpact.Application.GitHub.Dto;
using CodeImpact.Domain.Entities;
using CodeImpact.Domain.Repositories;
using MediatR;

namespace CodeImpact.Application.BackgroundJobs.Queries;

public sealed class GetBackgroundJobStatusQueryHandler : IRequestHandler<GetBackgroundJobStatusQuery, BackgroundJobStatusDto?>
{
    private readonly IBackgroundJobExecutionRepository _jobRepository;

    public GetBackgroundJobStatusQueryHandler(IBackgroundJobExecutionRepository jobRepository)
    {
        _jobRepository = jobRepository;
    }

    public async Task<BackgroundJobStatusDto?> Handle(GetBackgroundJobStatusQuery request, CancellationToken cancellationToken)
    {
        var execution = await _jobRepository.GetByIdForUserAsync(request.UserId, request.TaskId);
        if (execution is null)
        {
            return null;
        }

        Guid? reportId = null;
        ContributionSummaryDto? contributionSummary = null;

        if (execution.Status == BackgroundJobExecutionStatus.Succeeded && !string.IsNullOrWhiteSpace(execution.ResultJson))
        {
            if (execution.JobType == BackgroundJobExecutionType.ExecutiveReport)
            {
                reportId = TryParseReportId(execution.ResultJson);
            }

            if (execution.JobType == BackgroundJobExecutionType.ContributionSummary)
            {
                contributionSummary = JsonSerializer.Deserialize<ContributionSummaryDto>(execution.ResultJson);
            }
        }

        return new BackgroundJobStatusDto(
            execution.Id,
            execution.JobType,
            execution.Status,
            execution.CreatedAt,
            execution.StartedAt,
            execution.CompletedAt,
            execution.ErrorMessage,
            execution.HangfireJobId,
            reportId,
            contributionSummary);
    }

    private static Guid? TryParseReportId(string resultJson)
    {
        try
        {
            using var document = JsonDocument.Parse(resultJson);
            if (!document.RootElement.TryGetProperty("reportId", out var reportIdElement)
                && !document.RootElement.TryGetProperty("ReportId", out reportIdElement))
            {
                return null;
            }

            if (reportIdElement.ValueKind == JsonValueKind.String)
            {
                var reportIdString = reportIdElement.GetString();
                return Guid.TryParse(reportIdString, out var reportId) ? reportId : null;
            }

            return reportIdElement.ValueKind == JsonValueKind.Null
                ? null
                : reportIdElement.GetGuid();
        }
        catch
        {
            return null;
        }
    }
}
