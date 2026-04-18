namespace CodeImpact.Application.GitHub.Dto;

public sealed record ContributionDetailDto(
    Guid Id,
    string Type,
    long RepositoryId,
    string RepositoryFullName,
    string ExternalReference,
    string Title,
    string Author,
    DateTime OccurredAt,
    string Status,
    string Url,
    bool? IsApproved,
    IReadOnlyCollection<ContributionEvidenceDto> Evidence);

public sealed record ContributionEvidenceDto(
    string EvidenceType,
    string ExternalReference,
    string Actor,
    string State,
    DateTime OccurredAt,
    string Url);
