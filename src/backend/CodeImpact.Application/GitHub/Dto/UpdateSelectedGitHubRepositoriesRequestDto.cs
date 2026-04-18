using System.Collections.Generic;

namespace CodeImpact.Application.GitHub.Dto
{
    public sealed record UpdateSelectedGitHubRepositoriesRequestDto(IReadOnlyCollection<SelectedGitHubRepositoryDto> Repositories);
}
