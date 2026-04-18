using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using CodeImpact.Application.Common.Interfaces;
using CodeImpact.Application.GitHub.Dto;
using CodeImpact.Infrastructure.Settings;
using Microsoft.Extensions.Options;

namespace CodeImpact.Infrastructure.Services
{
    public class GitHubService : IGitHubService
    {
        private readonly HttpClient _httpClient;
        private readonly GitHubSettings _settings;
        private readonly IGitHubTokenProtector _tokenProtector;

        public GitHubService(
            HttpClient httpClient,
            IOptions<GitHubSettings> settings,
            IGitHubTokenProtector tokenProtector)
        {
            _httpClient = httpClient;
            _settings = settings.Value;
            _tokenProtector = tokenProtector;
            _httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("CodeImpact", "1.0"));
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public Task<string> GetAuthorizationUrlAsync()
        {
            var uri = new UriBuilder("https://github.com/login/oauth/authorize")
            {
                Query = $"client_id={_settings.ClientId}&redirect_uri={Uri.EscapeDataString(_settings.RedirectUri)}&scope={Uri.EscapeDataString(_settings.Scope)}"
            };

            return Task.FromResult(uri.ToString());
        }

        public async Task<GitHubCodeExchangeResultDto> ExchangeCodeAsync(string code)
        {
            var tokenRequest = new GitHubTokenRequest
            {
                ClientId = _settings.ClientId,
                ClientSecret = _settings.ClientSecret,
                Code = code,
                RedirectUri = _settings.RedirectUri
            };

            var tokenResponse = await _httpClient.PostAsJsonAsync("https://github.com/login/oauth/access_token", tokenRequest);
            tokenResponse.EnsureSuccessStatusCode();
            var tokenBody = await tokenResponse.Content.ReadFromJsonAsync<GitHubTokenResponse>();
            if (tokenBody is null || string.IsNullOrWhiteSpace(tokenBody.AccessToken))
            {
                throw new InvalidOperationException("Não foi possível trocar o código GitHub por token.");
            }

            var profileRequest = new HttpRequestMessage(HttpMethod.Get, "https://api.github.com/user");
            profileRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenBody.AccessToken);
            var profileResponse = await _httpClient.SendAsync(profileRequest);
            profileResponse.EnsureSuccessStatusCode();
            var profile = await profileResponse.Content.ReadFromJsonAsync<GitHubUserProfile>();
            if (profile is null)
            {
                throw new InvalidOperationException("Não foi possível carregar o perfil GitHub.");
            }

            var encryptedToken = _tokenProtector.Protect(tokenBody.AccessToken);
            return new GitHubCodeExchangeResultDto(profile.Login, profile.Id, encryptedToken);
        }

        public async Task<IEnumerable<GitHubRepositoryDto>> GetUserRepositoriesAsync(string encryptedAccessToken)
        {
            var accessToken = _tokenProtector.Unprotect(encryptedAccessToken);
            var request = new HttpRequestMessage(HttpMethod.Get, "https://api.github.com/user/repos?per_page=100");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var repos = await response.Content.ReadFromJsonAsync<IEnumerable<GitHubRepositoryResponse>>();
            if (repos is null)
            {
                return new List<GitHubRepositoryDto>();
            }

            var result = new List<GitHubRepositoryDto>();
            foreach (var repo in repos)
            {
                result.Add(new GitHubRepositoryDto(repo.Id, repo.Name, repo.FullName, repo.Private));
            }

            return result;
        }

        public async Task<IEnumerable<GitHubPullRequestDto>> GetPullRequestsAsync(string encryptedAccessToken, string repositoryFullName)
        {
            var accessToken = _tokenProtector.Unprotect(encryptedAccessToken);
            var request = new HttpRequestMessage(HttpMethod.Get, $"https://api.github.com/repos/{repositoryFullName}/pulls?state=all&per_page=100");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var pullRequests = await response.Content.ReadFromJsonAsync<IEnumerable<GitHubPullRequestResponse>>();
            if (pullRequests is null)
            {
                return new List<GitHubPullRequestDto>();
            }

            return pullRequests
                .Select(pr => new GitHubPullRequestDto(pr.Id, pr.Title, pr.State, pr.HtmlUrl))
                .ToList();
        }

        private sealed record GitHubTokenRequest
        {
            [JsonPropertyName("client_id")] public string ClientId { get; init; } = string.Empty;
            [JsonPropertyName("client_secret")] public string ClientSecret { get; init; } = string.Empty;
            [JsonPropertyName("code")] public string Code { get; init; } = string.Empty;
            [JsonPropertyName("redirect_uri")] public string RedirectUri { get; init; } = string.Empty;
        }

        private sealed record GitHubTokenResponse(
            [property: JsonPropertyName("access_token")] string AccessToken,
            [property: JsonPropertyName("scope")] string Scope,
            [property: JsonPropertyName("token_type")] string TokenType);

        private sealed record GitHubUserProfile(
            [property: JsonPropertyName("login")] string Login,
            [property: JsonPropertyName("id")] long Id);

        private sealed record GitHubRepositoryResponse(
            [property: JsonPropertyName("id")] long Id,
            [property: JsonPropertyName("name")] string Name,
            [property: JsonPropertyName("full_name")] string FullName,
            [property: JsonPropertyName("private")] bool Private);

        private sealed record GitHubPullRequestResponse(
            [property: JsonPropertyName("id")] long Id,
            [property: JsonPropertyName("title")] string Title,
            [property: JsonPropertyName("state")] string State,
            [property: JsonPropertyName("html_url")] string HtmlUrl);
    }
}
