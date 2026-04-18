namespace CodeImpact.Application.GitHub.Dto
{
    public sealed record GitHubCodeExchangeResultDto(string GitHubUsername, long GitHubUserId, string EncryptedAccessToken);
}
