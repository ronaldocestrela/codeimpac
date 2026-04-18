using System;

namespace CodeImpact.Application.GitHub.Dto
{
    public sealed record GitHubAccountDto(Guid Id, string GitHubUsername, long GitHubUserId, DateTime LinkedAt);
}
