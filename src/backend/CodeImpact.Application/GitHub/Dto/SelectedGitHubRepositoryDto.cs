namespace CodeImpact.Application.GitHub.Dto
{
    public sealed record SelectedGitHubRepositoryDto(long Id, string Name, string FullName, bool Private);
}
