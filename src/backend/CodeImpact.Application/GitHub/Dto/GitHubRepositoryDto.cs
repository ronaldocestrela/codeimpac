namespace CodeImpact.Application.GitHub.Dto
{
    public sealed record GitHubRepositoryDto(
        long Id,
        string Name,
        string FullName,
        bool Private,
        string OwnerLogin,
        string OwnerType);
}
