namespace CodeImpact.Application.GitHub.Dto;

public sealed record ContributionListItemDto(
    Guid Id,
    string Type,
    long RepositoryId,
    string RepositoryFullName,
    string Title,
    string Author,
    DateTime OccurredAt,
    string Status,
    string Url,
    bool? IsApproved);
