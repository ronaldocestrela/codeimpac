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
                new GitHubRepositoryDto(1, "repo", "octocat/repo", false, "octocat", "User")
            })
        };

        var handler = new GetGitHubRepositoriesQueryHandler(service, repository);
        var result = await handler.Handle(new GetGitHubRepositoriesQuery(userId, null), CancellationToken.None);

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
        var commitRepository = new StubGitHubCommitRepository();
        var pullRequestRepository = new StubGitHubPullRequestRepository();
        var reviewRepository = new StubGitHubPullRequestReviewRepository();

        var handler = new SyncGitHubRepositoryCommandHandler(
            accountRepository,
            selectionRepository,
            service,
            commitRepository,
            pullRequestRepository,
            reviewRepository);

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

        var handler = new SyncGitHubRepositoryCommandHandler(
            accountRepository,
            selectionRepository,
            new StubGitHubService(),
            new StubGitHubCommitRepository(),
            new StubGitHubPullRequestRepository(),
            new StubGitHubPullRequestReviewRepository());

        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await handler.Handle(new SyncGitHubRepositoryCommand(userId, 999), CancellationToken.None));
    }

    [Fact]
    public async Task SyncGitHubRepositoryCommandHandler_PersistsContributions_WhenRepositoryIsSelected()
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
                new GitHubRepositorySelection(userId, account.Id, repositoryId, "repo", "octocat/repo", false, "octocat", "User"))
        };

        var service = new StubGitHubService
        {
            GetPullRequestsAsyncDelegate = (_, repositoryFullName) =>
            {
                syncedRepository = repositoryFullName;
                return Task.FromResult<IEnumerable<GitHubPullRequestDto>>(new[]
                {
                    new GitHubPullRequestDto(
                        500,
                        42,
                        "Improve dashboard",
                        "open",
                        "octocat",
                        DateTime.UtcNow,
                        null,
                        null,
                        "https://github.com/octocat/repo/pull/42")
                });
            },
            GetPullRequestReviewsAsyncDelegate = (_, _, _) =>
            {
                return Task.FromResult<IEnumerable<GitHubPullRequestReviewDto>>(new[]
                {
                    new GitHubPullRequestReviewDto(
                        900,
                        "reviewer-a",
                        "APPROVED",
                        DateTime.UtcNow,
                        "https://github.com/octocat/repo/pull/42#pullrequestreview-900")
                });
            },
            GetCommitsAsyncDelegate = (_, _) =>
            {
                return Task.FromResult<IEnumerable<GitHubCommitDto>>(new[]
                {
                    new GitHubCommitDto(
                        "abc123",
                        "feat: add dashboard",
                        "Octo Cat",
                        "octo@example.com",
                        DateTime.UtcNow,
                        "https://github.com/octocat/repo/commit/abc123")
                });
            }
        };

        var commitRepository = new StubGitHubCommitRepository();
        var pullRequestRepository = new StubGitHubPullRequestRepository();
        var reviewRepository = new StubGitHubPullRequestReviewRepository();

        var handler = new SyncGitHubRepositoryCommandHandler(
            accountRepository,
            selectionRepository,
            service,
            commitRepository,
            pullRequestRepository,
            reviewRepository);

        await handler.Handle(new SyncGitHubRepositoryCommand(userId, 100), CancellationToken.None);

        Assert.Equal("octocat/repo", syncedRepository);
        Assert.Single(pullRequestRepository.AddedPullRequests);
        Assert.True(pullRequestRepository.AddedPullRequests[0].IsApproved);
        Assert.Single(reviewRepository.AddedReviews);
        Assert.Single(commitRepository.AddedCommits);
    }

    [Fact]
    public async Task SyncGitHubRepositoryCommandHandler_MarksPullRequestAsNotApproved_WhenNoApprovedReviewExists()
    {
        var userId = Guid.NewGuid();
        var account = new GitHubAccount(userId, "octocat", 123, "encrypted-token");
        var accountRepository = new StubGitHubAccountRepository
        {
            GetByUserIdAsyncDelegate = _ => Task.FromResult<GitHubAccount?>(account)
        };

        var selectionRepository = new StubGitHubRepositorySelectionRepository
        {
            GetByUserAndRepositoryIdAsyncDelegate = (_, repositoryId) => Task.FromResult<GitHubRepositorySelection?>(
                new GitHubRepositorySelection(userId, account.Id, repositoryId, "repo", "octocat/repo", false, "octocat", "User"))
        };

        var service = new StubGitHubService
        {
            GetPullRequestsAsyncDelegate = (_, _) => Task.FromResult<IEnumerable<GitHubPullRequestDto>>(new[]
            {
                new GitHubPullRequestDto(500, 42, "Improve dashboard", "open", "octocat", DateTime.UtcNow, null, null, "https://github.com/octocat/repo/pull/42")
            }),
            GetPullRequestReviewsAsyncDelegate = (_, _, _) => Task.FromResult<IEnumerable<GitHubPullRequestReviewDto>>(new[]
            {
                new GitHubPullRequestReviewDto(901, "reviewer-a", "COMMENTED", DateTime.UtcNow, "https://github.com/octocat/repo/pull/42#pullrequestreview-901")
            }),
            GetCommitsAsyncDelegate = (_, _) => Task.FromResult<IEnumerable<GitHubCommitDto>>(Array.Empty<GitHubCommitDto>())
        };

        var pullRequestRepository = new StubGitHubPullRequestRepository();

        var handler = new SyncGitHubRepositoryCommandHandler(
            accountRepository,
            selectionRepository,
            service,
            new StubGitHubCommitRepository(),
            pullRequestRepository,
            new StubGitHubPullRequestReviewRepository());

        await handler.Handle(new SyncGitHubRepositoryCommand(userId, 100), CancellationToken.None);

        Assert.Single(pullRequestRepository.AddedPullRequests);
        Assert.False(pullRequestRepository.AddedPullRequests[0].IsApproved);
    }

    [Fact]
    public async Task GetContributionsQueryHandler_ReturnsUnifiedContributionsOrderedByDate()
    {
        var userId = Guid.NewGuid();
        var newerDate = DateTime.UtcNow;
        var olderDate = newerDate.AddDays(-1);

        var commitRepository = new StubGitHubCommitRepository
        {
            ListByUserAsyncDelegate = (_, _, _, _, _) => Task.FromResult<IReadOnlyCollection<GitHubCommit>>(new[]
            {
                new GitHubCommit(userId, Guid.NewGuid(), 100, "octocat/repo", "abc123", "feat: add dashboard", "Octo Cat", "octo@example.com", olderDate, "https://github.com/octocat/repo/commit/abc123")
            })
        };

        var pullRequestRepository = new StubGitHubPullRequestRepository
        {
            ListByUserAsyncDelegate = (_, _, _, _, _) => Task.FromResult<IReadOnlyCollection<GitHubPullRequest>>(new[]
            {
                new GitHubPullRequest(userId, Guid.NewGuid(), 100, "octocat/repo", 500, 42, "Improve dashboard", "open", "octocat", true, newerDate, null, null, "https://github.com/octocat/repo/pull/42")
            })
        };

        var handler = new GetContributionsQueryHandler(commitRepository, pullRequestRepository);

        var result = await handler.Handle(new GetContributionsQuery(userId, null, null, null, null, 1, 20), CancellationToken.None);

        Assert.Equal(2, result.TotalCount);
        Assert.Equal(1, result.Page);
        Assert.Equal(1, result.TotalPages);
        Assert.Equal("pull_request", result.Items.First().Type);
        Assert.Equal("approved", result.Items.First().Status);
        Assert.Equal("commit", result.Items.Last().Type);
    }

    [Fact]
    public async Task GetContributionsQueryHandler_AppliesPaginationCorrectly()
    {
        var userId = Guid.NewGuid();
        var now = DateTime.UtcNow;

        var commitRepository = new StubGitHubCommitRepository
        {
            ListByUserAsyncDelegate = (_, _, _, _, _) => Task.FromResult<IReadOnlyCollection<GitHubCommit>>(new[]
            {
                new GitHubCommit(userId, Guid.NewGuid(), 100, "octocat/repo", "sha-1", "commit-1", "Octo", "octo@example.com", now.AddMinutes(-3), "https://github.com/octocat/repo/commit/sha-1"),
                new GitHubCommit(userId, Guid.NewGuid(), 100, "octocat/repo", "sha-2", "commit-2", "Octo", "octo@example.com", now.AddMinutes(-2), "https://github.com/octocat/repo/commit/sha-2"),
                new GitHubCommit(userId, Guid.NewGuid(), 100, "octocat/repo", "sha-3", "commit-3", "Octo", "octo@example.com", now.AddMinutes(-1), "https://github.com/octocat/repo/commit/sha-3")
            })
        };

        var handler = new GetContributionsQueryHandler(commitRepository, new StubGitHubPullRequestRepository());

        var result = await handler.Handle(new GetContributionsQuery(userId, null, null, null, null, 2, 2), CancellationToken.None);

        Assert.Equal(3, result.TotalCount);
        Assert.Equal(2, result.TotalPages);
        Assert.Equal(2, result.Page);
        Assert.Single(result.Items);
        Assert.Equal("commit-1", result.Items.Single().Title);
        Assert.True(result.HasPreviousPage);
        Assert.False(result.HasNextPage);
    }

    [Fact]
    public async Task GetContributionsQueryHandler_Throws_WhenDateRangeIsInvalid()
    {
        var userId = Guid.NewGuid();
        var handler = new GetContributionsQueryHandler(new StubGitHubCommitRepository(), new StubGitHubPullRequestRepository());

        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await handler.Handle(
                new GetContributionsQuery(userId, null, null, DateTime.UtcNow, DateTime.UtcNow.AddDays(-1), 1, 20),
                CancellationToken.None));
    }

    [Fact]
    public async Task GetPullRequestContributionDetailQueryHandler_ReturnsReviewEvidence()
    {
        var userId = Guid.NewGuid();
        var pullRequest = new GitHubPullRequest(
            userId,
            Guid.NewGuid(),
            100,
            "octocat/repo",
            500,
            42,
            "Improve dashboard",
            "open",
            "octocat",
            true,
            DateTime.UtcNow,
            null,
            null,
            "https://github.com/octocat/repo/pull/42");

        var pullRequestRepository = new StubGitHubPullRequestRepository
        {
            GetByIdAsyncDelegate = (_, _) => Task.FromResult<GitHubPullRequest?>(pullRequest)
        };

        var reviewRepository = new StubGitHubPullRequestReviewRepository
        {
            ListByPullRequestAsyncDelegate = (_, _, _) => Task.FromResult<IReadOnlyCollection<GitHubPullRequestReview>>(new[]
            {
                new GitHubPullRequestReview(
                    userId,
                    Guid.NewGuid(),
                    100,
                    "octocat/repo",
                    500,
                    900,
                    "reviewer-a",
                    "APPROVED",
                    DateTime.UtcNow,
                    "https://github.com/octocat/repo/pull/42#pullrequestreview-900")
            })
        };

        var handler = new GetPullRequestContributionDetailQueryHandler(pullRequestRepository, reviewRepository);

        var result = await handler.Handle(new GetPullRequestContributionDetailQuery(userId, pullRequest.Id), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("approved", result!.Status);
        Assert.Equal(2, result.Evidence.Count);
        Assert.Contains(result.Evidence, evidence => evidence.EvidenceType == "pull_request_review");
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
                    new SelectedGitHubRepositoryDto(1, "repo-1", "octocat/repo-1", false, "octocat", "User"),
                    new SelectedGitHubRepositoryDto(2, "repo-2", "octocat/repo-2", true, "octocat", "User")
                },
                null),
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
                new GitHubRepositorySelection(userId, Guid.NewGuid(), 100, "repo", "octocat/repo", false, "octocat", "User")
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
                            @private = false,
                            owner = new { login = "octocat", type = "User" }
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
                Assert.Equal("octocat", repo.OwnerLogin);
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
        public Func<string, Task<IEnumerable<GitHubOrganizationDto>>> GetUserOrganizationsAsyncDelegate { get; set; } = _ => Task.FromResult<IEnumerable<GitHubOrganizationDto>>(Array.Empty<GitHubOrganizationDto>());
        public Func<string, string, Task<IEnumerable<GitHubPullRequestDto>>> GetPullRequestsAsyncDelegate { get; set; } = (_, _) => Task.FromResult<IEnumerable<GitHubPullRequestDto>>(Array.Empty<GitHubPullRequestDto>());
        public Func<string, string, Task<IEnumerable<GitHubCommitDto>>> GetCommitsAsyncDelegate { get; set; } = (_, _) => Task.FromResult<IEnumerable<GitHubCommitDto>>(Array.Empty<GitHubCommitDto>());
        public Func<string, string, int, Task<IEnumerable<GitHubPullRequestReviewDto>>> GetPullRequestReviewsAsyncDelegate { get; set; } = (_, _, _) => Task.FromResult<IEnumerable<GitHubPullRequestReviewDto>>(Array.Empty<GitHubPullRequestReviewDto>());

        public Task<string> GetAuthorizationUrlAsync() => Task.FromResult(string.Empty);
        public Task<GitHubCodeExchangeResultDto> ExchangeCodeAsync(string code) => ExchangeCodeAsyncDelegate(code);
        public Task<IEnumerable<GitHubRepositoryDto>> GetUserRepositoriesAsync(string encryptedAccessToken) => GetUserRepositoriesAsyncDelegate(encryptedAccessToken);
        public Task<IEnumerable<GitHubOrganizationDto>> GetUserOrganizationsAsync(string encryptedAccessToken) => GetUserOrganizationsAsyncDelegate(encryptedAccessToken);
        public Task<IEnumerable<GitHubPullRequestDto>> GetPullRequestsAsync(string encryptedAccessToken, string repositoryFullName)
            => GetPullRequestsAsyncDelegate(encryptedAccessToken, repositoryFullName);
        public Task<IEnumerable<GitHubCommitDto>> GetCommitsAsync(string encryptedAccessToken, string repositoryFullName)
            => GetCommitsAsyncDelegate(encryptedAccessToken, repositoryFullName);
        public Task<IEnumerable<GitHubPullRequestReviewDto>> GetPullRequestReviewsAsync(string encryptedAccessToken, string repositoryFullName, int pullRequestNumber)
            => GetPullRequestReviewsAsyncDelegate(encryptedAccessToken, repositoryFullName, pullRequestNumber);
    }

    private sealed class StubGitHubCommitRepository : IGitHubCommitRepository
    {
        public List<GitHubCommit> AddedCommits { get; } = new();
        public Func<Guid, long?, string?, DateTime?, DateTime?, Task<IReadOnlyCollection<GitHubCommit>>> ListByUserAsyncDelegate { get; set; }
            = (_, _, _, _, _) => Task.FromResult<IReadOnlyCollection<GitHubCommit>>(Array.Empty<GitHubCommit>());
        public Func<Guid, Guid, Task<GitHubCommit?>> GetByIdAsyncDelegate { get; set; }
            = (_, _) => Task.FromResult<GitHubCommit?>(null);

        public Task<GitHubCommit?> GetByUserRepositoryAndShaAsync(Guid userId, long repositoryId, string commitSha)
            => Task.FromResult<GitHubCommit?>(null);

        public Task<IReadOnlyCollection<GitHubCommit>> ListByUserAsync(Guid userId, long? repositoryId, string? organizationLogin, DateTime? from, DateTime? to)
            => ListByUserAsyncDelegate(userId, repositoryId, organizationLogin, from, to);

        public Task<GitHubCommit?> GetByIdAsync(Guid userId, Guid commitId)
            => GetByIdAsyncDelegate(userId, commitId);

        public Task AddAsync(GitHubCommit commit)
        {
            AddedCommits.Add(commit);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(GitHubCommit commit) => Task.CompletedTask;
    }

    private sealed class StubGitHubPullRequestRepository : IGitHubPullRequestRepository
    {
        public List<GitHubPullRequest> AddedPullRequests { get; } = new();
        public Func<Guid, long?, string?, DateTime?, DateTime?, Task<IReadOnlyCollection<GitHubPullRequest>>> ListByUserAsyncDelegate { get; set; }
            = (_, _, _, _, _) => Task.FromResult<IReadOnlyCollection<GitHubPullRequest>>(Array.Empty<GitHubPullRequest>());
        public Func<Guid, Guid, Task<GitHubPullRequest?>> GetByIdAsyncDelegate { get; set; }
            = (_, _) => Task.FromResult<GitHubPullRequest?>(null);

        public Task<GitHubPullRequest?> GetByUserRepositoryAndGitHubPullRequestIdAsync(Guid userId, long repositoryId, long gitHubPullRequestId)
            => Task.FromResult<GitHubPullRequest?>(null);

        public Task<IReadOnlyCollection<GitHubPullRequest>> ListByUserAsync(Guid userId, long? repositoryId, string? organizationLogin, DateTime? from, DateTime? to)
            => ListByUserAsyncDelegate(userId, repositoryId, organizationLogin, from, to);

        public Task<GitHubPullRequest?> GetByIdAsync(Guid userId, Guid pullRequestId)
            => GetByIdAsyncDelegate(userId, pullRequestId);

        public Task AddAsync(GitHubPullRequest pullRequest)
        {
            AddedPullRequests.Add(pullRequest);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(GitHubPullRequest pullRequest) => Task.CompletedTask;
    }

    private sealed class StubGitHubPullRequestReviewRepository : IGitHubPullRequestReviewRepository
    {
        public List<GitHubPullRequestReview> AddedReviews { get; } = new();
        public Func<Guid, long, long, Task<IReadOnlyCollection<GitHubPullRequestReview>>> ListByPullRequestAsyncDelegate { get; set; }
            = (_, _, _) => Task.FromResult<IReadOnlyCollection<GitHubPullRequestReview>>(Array.Empty<GitHubPullRequestReview>());

        public Task<GitHubPullRequestReview?> GetByGitHubReviewIdAsync(long gitHubReviewId)
            => Task.FromResult<GitHubPullRequestReview?>(null);

        public Task<IReadOnlyCollection<GitHubPullRequestReview>> ListByPullRequestAsync(Guid userId, long repositoryId, long gitHubPullRequestId)
            => ListByPullRequestAsyncDelegate(userId, repositoryId, gitHubPullRequestId);

        public Task AddAsync(GitHubPullRequestReview review)
        {
            AddedReviews.Add(review);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(GitHubPullRequestReview review) => Task.CompletedTask;
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

        public Task ReplaceForUserAsync(Guid userId, Guid gitHubAccountId, IEnumerable<GitHubRepositorySelection> selections, string? ownerLoginScope = null)
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
