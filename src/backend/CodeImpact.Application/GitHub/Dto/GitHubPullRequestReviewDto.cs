namespace CodeImpact.Application.GitHub.Dto;

public sealed record GitHubPullRequestReviewDto(
    long Id,
    string ReviewerLogin,
    string State,
    DateTime SubmittedAt,
    string Url);
