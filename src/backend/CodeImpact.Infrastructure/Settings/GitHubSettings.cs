namespace CodeImpact.Infrastructure.Settings
{
    public sealed class GitHubSettings
    {
        public string ClientId { get; set; } = string.Empty;
        public string ClientSecret { get; set; } = string.Empty;
        public string RedirectUri { get; set; } = string.Empty;
        public string Scope { get; set; } = "read:user repo";
    }
}
