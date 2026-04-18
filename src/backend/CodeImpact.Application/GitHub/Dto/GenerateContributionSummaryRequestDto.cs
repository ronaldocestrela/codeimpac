namespace CodeImpact.Application.GitHub.Dto;

public sealed record GenerateContributionSummaryRequestDto(
    long? RepositoryId,
    DateTime? From,
    DateTime? To);