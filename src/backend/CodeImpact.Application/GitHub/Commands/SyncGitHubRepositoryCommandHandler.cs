using System.Threading;
using System.Threading.Tasks;
using CodeImpact.Application.Common.Interfaces;
using CodeImpact.Application.GitHub.Commands;
using CodeImpact.Domain.Entities;
using CodeImpact.Domain.Repositories;
using MediatR;

namespace CodeImpact.Application.GitHub.Commands
{
    public class SyncGitHubRepositoryCommandHandler : IRequestHandler<SyncGitHubRepositoryCommand>
    {
        private readonly IGitHubAccountRepository _gitHubAccountRepository;
        private readonly IGitHubRepositorySelectionRepository _selectionRepository;
        private readonly IGitHubService _gitHubService;
        private readonly IGitHubCommitRepository _commitRepository;
        private readonly IGitHubPullRequestRepository _pullRequestRepository;
        private readonly IGitHubPullRequestReviewRepository _pullRequestReviewRepository;

        public SyncGitHubRepositoryCommandHandler(
            IGitHubAccountRepository gitHubAccountRepository,
            IGitHubRepositorySelectionRepository selectionRepository,
            IGitHubService gitHubService,
            IGitHubCommitRepository commitRepository,
            IGitHubPullRequestRepository pullRequestRepository,
            IGitHubPullRequestReviewRepository pullRequestReviewRepository)
        {
            _gitHubAccountRepository = gitHubAccountRepository;
            _selectionRepository = selectionRepository;
            _gitHubService = gitHubService;
            _commitRepository = commitRepository;
            _pullRequestRepository = pullRequestRepository;
            _pullRequestReviewRepository = pullRequestReviewRepository;
        }

        public async Task<Unit> Handle(SyncGitHubRepositoryCommand request, CancellationToken cancellationToken)
        {
            var account = await _gitHubAccountRepository.GetByUserIdAsync(request.UserId);
            if (account is null)
            {
                throw new InvalidOperationException("Conta GitHub não encontrada para este usuário.");
            }

            var selection = await _selectionRepository.GetByUserAndRepositoryIdAsync(request.UserId, request.RepositoryId);
            if (selection is null)
            {
                throw new InvalidOperationException("Repositório não está selecionado para sincronização.");
            }

            var pullRequests = await _gitHubService.GetPullRequestsAsync(account.EncryptedAccessToken, selection.FullName);
            foreach (var pullRequest in pullRequests)
            {
                var reviews = await _gitHubService.GetPullRequestReviewsAsync(account.EncryptedAccessToken, selection.FullName, pullRequest.Number);
                var isApproved = reviews.Any(r => string.Equals(r.State, "APPROVED", StringComparison.OrdinalIgnoreCase));

                foreach (var review in reviews)
                {
                    var existingReview = await _pullRequestReviewRepository.GetByGitHubReviewIdAsync(review.Id);
                    if (existingReview is null)
                    {
                        var newReview = new GitHubPullRequestReview(
                            request.UserId,
                            account.Id,
                            selection.RepositoryId,
                            selection.FullName,
                            pullRequest.Id,
                            review.Id,
                            review.ReviewerLogin,
                            review.State,
                            review.SubmittedAt,
                            review.Url);
                        await _pullRequestReviewRepository.AddAsync(newReview);
                    }
                    else
                    {
                        existingReview.UpdateFromSync(review.ReviewerLogin, review.State, review.SubmittedAt, review.Url);
                        await _pullRequestReviewRepository.UpdateAsync(existingReview);
                    }
                }

                var existingPullRequest = await _pullRequestRepository.GetByUserRepositoryAndGitHubPullRequestIdAsync(
                    request.UserId,
                    selection.RepositoryId,
                    pullRequest.Id);

                if (existingPullRequest is null)
                {
                    var newPullRequest = new GitHubPullRequest(
                        request.UserId,
                        account.Id,
                        selection.RepositoryId,
                        selection.FullName,
                        pullRequest.Id,
                        pullRequest.Number,
                        pullRequest.Title,
                        pullRequest.State,
                        pullRequest.AuthorLogin,
                        isApproved,
                        pullRequest.CreatedAt,
                        pullRequest.ClosedAt,
                        pullRequest.MergedAt,
                        pullRequest.Url);
                    await _pullRequestRepository.AddAsync(newPullRequest);
                }
                else
                {
                    existingPullRequest.UpdateFromSync(
                        pullRequest.Title,
                        pullRequest.State,
                        pullRequest.AuthorLogin,
                        isApproved,
                        pullRequest.CreatedAt,
                        pullRequest.ClosedAt,
                        pullRequest.MergedAt,
                        pullRequest.Url);
                    await _pullRequestRepository.UpdateAsync(existingPullRequest);
                }
            }

            var commits = await _gitHubService.GetCommitsAsync(account.EncryptedAccessToken, selection.FullName);
            foreach (var commit in commits)
            {
                var existingCommit = await _commitRepository.GetByUserRepositoryAndShaAsync(request.UserId, selection.RepositoryId, commit.Sha);
                if (existingCommit is null)
                {
                    var newCommit = new GitHubCommit(
                        request.UserId,
                        account.Id,
                        selection.RepositoryId,
                        selection.FullName,
                        commit.Sha,
                        commit.Message,
                        commit.AuthorName,
                        commit.AuthorEmail,
                        commit.CommittedAt,
                        commit.Url);
                    await _commitRepository.AddAsync(newCommit);
                }
                else
                {
                    existingCommit.UpdateFromSync(
                        commit.Message,
                        commit.AuthorName,
                        commit.AuthorEmail,
                        commit.CommittedAt,
                        commit.Url);
                    await _commitRepository.UpdateAsync(existingCommit);
                }
            }

            return Unit.Value;
        }
    }
}
