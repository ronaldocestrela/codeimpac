namespace CodeImpact.Application.Reports.Dto;

public sealed record GenerateExecutiveReportRequestDto(
    long? RepositoryId,
    string? OrganizationLogin,
    DateTime? From,
    DateTime? To);
