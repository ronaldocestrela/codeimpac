using System.Collections.Generic;
using System.Threading.Tasks;
using CodeImpact.Application.GitHub.Dto;

namespace CodeImpact.Application.Common.Interfaces
{
    public interface IGitHubService
    {
        Task<string> GetAuthorizationUrlAsync();
        Task<GitHubCodeExchangeResultDto> ExchangeCodeAsync(string code);
        Task<IEnumerable<GitHubRepositoryDto>> GetUserRepositoriesAsync(string encryptedAccessToken);
        Task<IEnumerable<GitHubPullRequestDto>> GetPullRequestsAsync(string encryptedAccessToken, string repositoryFullName);
    }
}
