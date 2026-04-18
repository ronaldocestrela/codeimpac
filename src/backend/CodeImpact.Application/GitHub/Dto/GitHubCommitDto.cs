namespace CodeImpact.Application.GitHub.Dto;

public sealed record GitHubCommitDto(
    string Sha,
    string Message,
    string AuthorName,
    string AuthorEmail,
    DateTime CommittedAt,
    string Url);
