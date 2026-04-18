using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using CodeImpact.Application.Common.Interfaces;
using CodeImpact.Application.GitHub.Commands;
using CodeImpact.Application.GitHub.Dto;
using CodeImpact.Application.GitHub.Queries;
using CodeImpact.Domain.Entities;
using CodeImpact.Domain.Repositories;
using CodeImpact.Infrastructure.Services;
using CodeImpact.Infrastructure.Settings;
using Microsoft.Extensions.Options;
using Xunit;

namespace CodeImpact.Tests;

public class GitHubIntegrationTests
{
    [Fact]
    public async Task LinkGitHubAccountCommandHandler_AddsNewAccount_WhenNoExistingAccount()
    {
        var userId = Guid.NewGuid();
        var service = new StubGitHubService
        {
            ExchangeCodeAsyncDelegate = _ => Task.FromResult(new GitHubCodeExchangeResultDto("octocat", 123, "encrypted-token"))
        };

        var repository = new StubGitHubAccountRepository
        {
            GetByGitHubUserIdAsyncDelegate = _ => Task.FromResult<GitHubAccount?>(null)
        };

        var handler = new LinkGitHubAccountCommandHandler(service, repository);
        var result = await handler.Handle(new LinkGitHubAccountCommand(userId, "fake-code"), CancellationToken.None);

        Assert.NotNull(repository.AddedAccount);
        Assert.Equal(userId, repository.AddedAccount!.UserId);
        Assert.Equal("octocat", result.GitHubUsername);
        Assert.Equal(123, result.GitHubUserId);
    }

    [Fact]
    public async Task LinkGitHubAccountCommandHandler_UpdatesExistingAccount_WhenSameUser()
    {
        var userId = Guid.NewGuid();
        var existingAccount = new GitHubAccount(userId, "octocat", 123, "old-token");
        var service = new StubGitHubService
        {
            ExchangeCodeAsyncDelegate = _ => Task.FromResult(new GitHubCodeExchangeResultDto("octocat", 123, "new-encrypted-token"))
        };

        var repository = new StubGitHubAccountRepository
        {
            GetByGitHubUserIdAsyncDelegate = _ => Task.FromResult<GitHubAccount?>(existingAccount)
        };

        var handler = new LinkGitHubAccountCommandHandler(service, repository);
        var result = await handler.Handle(new LinkGitHubAccountCommand(userId, "fake-code"), CancellationToken.None);

        Assert.NotNull(repository.UpdatedAccount);
        Assert.Equal("new-encrypted-token", repository.UpdatedAccount!.EncryptedAccessToken);
        Assert.Equal(existingAccount.Id, result.Id);
    }

    [Fact]
    public async Task LinkGitHubAccountCommandHandler_Throws_WhenGitHubAccountLinkedToDifferentUser()
    {
        var userId = Guid.NewGuid();
        var existingAccount = new GitHubAccount(Guid.NewGuid(), "octocat", 123, "old-token");
        var service = new StubGitHubService
        {
            ExchangeCodeAsyncDelegate = _ => Task.FromResult(new GitHubCodeExchangeResultDto("octocat", 123, "new-encrypted-token"))
        };

        var repository = new StubGitHubAccountRepository
        {
            GetByGitHubUserIdAsyncDelegate = _ => Task.FromResult<GitHubAccount?>(existingAccount)
        };

        var handler = new LinkGitHubAccountCommandHandler(service, repository);

        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await handler.Handle(new LinkGitHubAccountCommand(userId, "fake-code"), CancellationToken.None));
    }

    [Fact]
    public async Task GetGitHubRepositoriesQueryHandler_ReturnsRepositories_WhenAccountExists()
    {
        var userId = Guid.NewGuid();
        var account = new GitHubAccount(userId, "octocat", 123, "encrypted-token");
        var repository = new StubGitHubAccountRepository
        {
            GetByUserIdAsyncDelegate = _ => Task.FromResult<GitHubAccount?>(account)
        };

        var service = new StubGitHubService
        {
            GetUserRepositoriesAsyncDelegate = _ => Task.FromResult<IEnumerable<GitHubRepositoryDto>>(new[]
            {
                new GitHubRepositoryDto(1, "repo", "octocat/repo", false)
            })
        };

        var handler = new GetGitHubRepositoriesQueryHandler(service, repository);
        var result = await handler.Handle(new GetGitHubRepositoriesQuery(userId), CancellationToken.None);

        Assert.Collection(result,
            repo =>
            {
                Assert.Equal(1, repo.Id);
                Assert.Equal("octocat/repo", repo.FullName);
            });
    }

    [Fact]
    public async Task SyncGitHubRepositoryCommandHandler_Throws_WhenNoAccountExists()
    {
        var userId = Guid.NewGuid();
        var accountRepository = new StubGitHubAccountRepository
        {
            GetByUserIdAsyncDelegate = _ => Task.FromResult<GitHubAccount?>(null)
        };
        var selectionRepository = new StubGitHubRepositorySelectionRepository();
        var service = new StubGitHubService();

        var handler = new SyncGitHubRepositoryCommandHandler(accountRepository, selectionRepository, service);

        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await handler.Handle(new SyncGitHubRepositoryCommand(userId, 123), CancellationToken.None));
    }

    [Fact]
    public async Task SyncGitHubRepositoryCommandHandler_Throws_WhenRepositoryIsNotSelected()
    {
        var userId = Guid.NewGuid();
        var accountRepository = new StubGitHubAccountRepository
        {
            GetByUserIdAsyncDelegate = _ => Task.FromResult<GitHubAccount?>(new GitHubAccount(userId, "octocat", 123, "encrypted-token"))
        };

        var selectionRepository = new StubGitHubRepositorySelectionRepository
        {
            GetByUserAndRepositoryIdAsyncDelegate = (_, _) => Task.FromResult<GitHubRepositorySelection?>(null)
        };

        var handler = new SyncGitHubRepositoryCommandHandler(accountRepository, selectionRepository, new StubGitHubService());

        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await handler.Handle(new SyncGitHubRepositoryCommand(userId, 999), CancellationToken.None));
    }

    [Fact]
    public async Task SyncGitHubRepositoryCommandHandler_CallsGitHubService_WhenRepositoryIsSelected()
    {
        var userId = Guid.NewGuid();
        string? syncedRepository = null;
        var account = new GitHubAccount(userId, "octocat", 123, "encrypted-token");
        var accountRepository = new StubGitHubAccountRepository
        {
            GetByUserIdAsyncDelegate = _ => Task.FromResult<GitHubAccount?>(account)
        };

        var selectionRepository = new StubGitHubRepositorySelectionRepository
        {
            GetByUserAndRepositoryIdAsyncDelegate = (_, repositoryId) => Task.FromResult<GitHubRepositorySelection?>(
                new GitHubRepositorySelection(userId, account.Id, repositoryId, "repo", "octocat/repo", false))
        };

        var service = new StubGitHubService
        {
            GetPullRequestsAsyncDelegate = (_, repositoryFullName) =>
            {
                syncedRepository = repositoryFullName;
                return Task.FromResult<IEnumerable<GitHubPullRequestDto>>(Array.Empty<GitHubPullRequestDto>());
            }
        };

        var handler = new SyncGitHubRepositoryCommandHandler(accountRepository, selectionRepository, service);

        await handler.Handle(new SyncGitHubRepositoryCommand(userId, 100), CancellationToken.None);

        Assert.Equal("octocat/repo", syncedRepository);
    }

    [Fact]
    public async Task UpdateSelectedGitHubRepositoriesCommandHandler_ReplacesSelections_WhenAccountExists()
    {
        var userId = Guid.NewGuid();
        var account = new GitHubAccount(userId, "octocat", 123, "encrypted-token");
        var accountRepository = new StubGitHubAccountRepository
        {
            GetByUserIdAsyncDelegate = _ => Task.FromResult<GitHubAccount?>(account)
        };

        var selectionRepository = new StubGitHubRepositorySelectionRepository();
        var handler = new UpdateSelectedGitHubRepositoriesCommandHandler(accountRepository, selectionRepository);

        await handler.Handle(
            new UpdateSelectedGitHubRepositoriesCommand(
                userId,
                new[]
                {
                    new SelectedGitHubRepositoryDto(1, "repo-1", "octocat/repo-1", false),
                    new SelectedGitHubRepositoryDto(2, "repo-2", "octocat/repo-2", true)
                }),
            CancellationToken.None);

        Assert.Equal(userId, selectionRepository.LastUserId);
        Assert.Equal(account.Id, selectionRepository.LastGitHubAccountId);
        Assert.Equal(2, selectionRepository.LastSelections.Count);
        Assert.Contains(selectionRepository.LastSelections, item => item.RepositoryId == 1);
        Assert.Contains(selectionRepository.LastSelections, item => item.RepositoryId == 2);
    }

    [Fact]
    public async Task GetSelectedGitHubRepositoriesQueryHandler_ReturnsMappedDtos()
    {
        var userId = Guid.NewGuid();
        var selectionRepository = new StubGitHubRepositorySelectionRepository
        {
            GetByUserIdAsyncDelegate = _ => Task.FromResult<IReadOnlyCollection<GitHubRepositorySelection>>(new[]
            {
                new GitHubRepositorySelection(userId, Guid.NewGuid(), 100, "repo", "octocat/repo", false)
            })
        };

        var handler = new GetSelectedGitHubRepositoriesQueryHandler(selectionRepository);
        var result = await handler.Handle(new GetSelectedGitHubRepositoriesQuery(userId), CancellationToken.None);

        Assert.Collection(result,
            item =>
            {
                Assert.Equal(100, item.Id);
                Assert.Equal("octocat/repo", item.FullName);
            });
    }

    [Fact]
    public async Task GitHubService_GetAuthorizationUrlAsync_ContainsClientIdRedirectUriAndScope()
    {
        var settings = new GitHubSettings
        {
            ClientId = "client-id",
            ClientSecret = "secret",
            RedirectUri = "https://localhost:5173/github/callback",
            Scope = "read:user repo"
        };

        var httpClient = new HttpClient(new TestHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)));
        var service = new GitHubService(httpClient, Options.Create(settings), new StubTokenProtector());

        var url = await service.GetAuthorizationUrlAsync();

        Assert.Contains("client_id=client-id", url);
        Assert.Contains("redirect_uri=https%3A%2F%2Flocalhost%3A5173%2Fgithub%2Fcallback", url);
        Assert.Contains("scope=read%3Auser%20repo", url);
    }

    [Fact]
    public async Task GitHubService_ExchangeCodeAsync_ReturnsProfileAndEncryptedToken()
    {
        var settings = new GitHubSettings
        {
            ClientId = "client-id",
            ClientSecret = "secret",
            RedirectUri = "https://localhost:5173/github/callback",
            Scope = "read:user repo"
        };

        var handler = new TestHttpMessageHandler(request =>
        {
            if (request.Method == HttpMethod.Post && request.RequestUri!.Host.Contains("github.com"))
            {
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = JsonContent.Create(new
                    {
                        access_token = "github-access-token",
                        scope = "repo",
                        token_type = "bearer"
                    })
                };
            }

            if (request.Method == HttpMethod.Get && request.RequestUri!.AbsolutePath == "/user")
            {
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = JsonContent.Create(new
                    {
                        login = "octocat",
                        id = 123L
                    })
                };
            }

            return new HttpResponseMessage(HttpStatusCode.NotFound);
        });

        var httpClient = new HttpClient(handler);
        var service = new GitHubService(httpClient, Options.Create(settings), new StubTokenProtector());

        var result = await service.ExchangeCodeAsync("fake-code");

        Assert.Equal("octocat", result.GitHubUsername);
        Assert.Equal(123, result.GitHubUserId);
        Assert.Equal("github-access-token", result.EncryptedAccessToken);
    }

    [Fact]
    public async Task GitHubService_GetUserRepositoriesAsync_MapsApiResponseToDto()
    {
        var settings = new GitHubSettings();
        var handler = new TestHttpMessageHandler(request =>
        {
            if (request.Method == HttpMethod.Get && request.RequestUri!.AbsolutePath.StartsWith("/user/repos"))
            {
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = JsonContent.Create(new[]
                    {
                        new
                        {
                            id = 100L,
                            name = "repo",
                            full_name = "octocat/repo",
                            @private = false
                        }
                    })
                };
            }

            return new HttpResponseMessage(HttpStatusCode.NotFound);
        });

        var httpClient = new HttpClient(handler);
        var service = new GitHubService(httpClient, Options.Create(settings), new StubTokenProtector());

        var result = await service.GetUserRepositoriesAsync("encrypted");

        Assert.Collection(result,
            repo =>
            {
                Assert.Equal(100, repo.Id);
                Assert.Equal("octocat/repo", repo.FullName);
                Assert.False(repo.Private);
            });
    }

    private sealed class StubGitHubAccountRepository : IGitHubAccountRepository
    {
        public Func<long, Task<GitHubAccount?>> GetByGitHubUserIdAsyncDelegate { get; set; } = _ => Task.FromResult<GitHubAccount?>(null);
        public Func<Guid, Task<GitHubAccount?>> GetByUserIdAsyncDelegate { get; set; } = _ => Task.FromResult<GitHubAccount?>(null);
        public GitHubAccount? AddedAccount { get; private set; }
        public GitHubAccount? UpdatedAccount { get; private set; }

        public Task AddAsync(GitHubAccount account)
        {
            AddedAccount = account;
            return Task.CompletedTask;
        }

        public Task<GitHubAccount?> GetByGitHubUserIdAsync(long gitHubUserId)
            => GetByGitHubUserIdAsyncDelegate(gitHubUserId);

        public Task<GitHubAccount?> GetByIdAsync(Guid id)
            => Task.FromResult<GitHubAccount?>(null);

        public Task<GitHubAccount?> GetByUserIdAsync(Guid userId)
            => GetByUserIdAsyncDelegate(userId);

        public Task UpdateAsync(GitHubAccount account)
        {
            UpdatedAccount = account;
            return Task.CompletedTask;
        }
    }

    private sealed class StubGitHubService : IGitHubService
    {
        public Func<string, Task<GitHubCodeExchangeResultDto>> ExchangeCodeAsyncDelegate { get; set; } = _ => throw new InvalidOperationException();
        public Func<string, Task<IEnumerable<GitHubRepositoryDto>>> GetUserRepositoriesAsyncDelegate { get; set; } = _ => Task.FromResult<IEnumerable<GitHubRepositoryDto>>(Array.Empty<GitHubRepositoryDto>());
        public Func<string, string, Task<IEnumerable<GitHubPullRequestDto>>> GetPullRequestsAsyncDelegate { get; set; } = (_, _) => Task.FromResult<IEnumerable<GitHubPullRequestDto>>(Array.Empty<GitHubPullRequestDto>());

        public Task<string> GetAuthorizationUrlAsync() => Task.FromResult(string.Empty);
        public Task<GitHubCodeExchangeResultDto> ExchangeCodeAsync(string code) => ExchangeCodeAsyncDelegate(code);
        public Task<IEnumerable<GitHubRepositoryDto>> GetUserRepositoriesAsync(string encryptedAccessToken) => GetUserRepositoriesAsyncDelegate(encryptedAccessToken);
        public Task<IEnumerable<GitHubPullRequestDto>> GetPullRequestsAsync(string encryptedAccessToken, string repositoryFullName)
            => GetPullRequestsAsyncDelegate(encryptedAccessToken, repositoryFullName);
    }

    private sealed class StubGitHubRepositorySelectionRepository : IGitHubRepositorySelectionRepository
    {
        public Func<Guid, Task<IReadOnlyCollection<GitHubRepositorySelection>>> GetByUserIdAsyncDelegate { get; set; }
            = _ => Task.FromResult<IReadOnlyCollection<GitHubRepositorySelection>>(Array.Empty<GitHubRepositorySelection>());
        public Func<Guid, long, Task<GitHubRepositorySelection?>> GetByUserAndRepositoryIdAsyncDelegate { get; set; }
            = (_, _) => Task.FromResult<GitHubRepositorySelection?>(null);

        public Guid LastUserId { get; private set; }
        public Guid LastGitHubAccountId { get; private set; }
        public IReadOnlyCollection<GitHubRepositorySelection> LastSelections { get; private set; } = Array.Empty<GitHubRepositorySelection>();

        public Task<IReadOnlyCollection<GitHubRepositorySelection>> GetByUserIdAsync(Guid userId)
            => GetByUserIdAsyncDelegate(userId);

        public Task<GitHubRepositorySelection?> GetByUserAndRepositoryIdAsync(Guid userId, long repositoryId)
            => GetByUserAndRepositoryIdAsyncDelegate(userId, repositoryId);

        public Task ReplaceForUserAsync(Guid userId, Guid gitHubAccountId, IEnumerable<GitHubRepositorySelection> selections)
        {
            LastUserId = userId;
            LastGitHubAccountId = gitHubAccountId;
            LastSelections = selections.ToList();
            return Task.CompletedTask;
        }
    }

    private sealed class StubTokenProtector : IGitHubTokenProtector
    {
        public string Protect(string value) => value;
        public string Unprotect(string value) => value;
    }

    private sealed class TestHttpMessageHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _handler;

        public TestHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> handler)
        {
            _handler = handler;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => Task.FromResult(_handler(request));
    }
}
