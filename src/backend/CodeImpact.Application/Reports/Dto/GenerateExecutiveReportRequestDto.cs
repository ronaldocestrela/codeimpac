namespace CodeImpact.Application.Reports.Dto;

public sealed record GenerateExecutiveReportRequestDto(
    long? RepositoryId,
    DateTime? From,
    DateTime? To);
