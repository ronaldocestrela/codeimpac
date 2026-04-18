using CodeImpact.Application.GitHub.Dto;
using CodeImpact.Domain.Repositories;
using MediatR;

namespace CodeImpact.Application.GitHub.Queries;

public sealed class GetPullRequestContributionDetailQueryHandler : IRequestHandler<GetPullRequestContributionDetailQuery, ContributionDetailDto?>
{
    private readonly IGitHubPullRequestRepository _pullRequestRepository;
    private readonly IGitHubPullRequestReviewRepository _reviewRepository;

    public GetPullRequestContributionDetailQueryHandler(
        IGitHubPullRequestRepository pullRequestRepository,
        IGitHubPullRequestReviewRepository reviewRepository)
    {
        _pullRequestRepository = pullRequestRepository;
        _reviewRepository = reviewRepository;
    }

    public async Task<ContributionDetailDto?> Handle(GetPullRequestContributionDetailQuery request, CancellationToken cancellationToken)
    {
        var pullRequest = await _pullRequestRepository.GetByIdAsync(request.UserId, request.ContributionId);
        if (pullRequest is null)
        {
            return null;
        }

        var reviews = await _reviewRepository.ListByPullRequestAsync(
            request.UserId,
            pullRequest.RepositoryId,
            pullRequest.GitHubPullRequestId);

        var status = ContributionStatusMapper.BuildPullRequestStatus(
            pullRequest.IsApproved,
            pullRequest.MergedAtGitHub,
            pullRequest.State);

        var evidence = new List<ContributionEvidenceDto>
        {
            new(
                "pull_request",
                pullRequest.GitHubPullRequestId.ToString(),
                pullRequest.AuthorLogin,
                status,
                pullRequest.CreatedAtGitHub,
                pullRequest.Url)
        };

        evidence.AddRange(reviews.Select(review =>
            new ContributionEvidenceDto(
                "pull_request_review",
                review.GitHubReviewId.ToString(),
                review.ReviewerLogin,
                review.State,
                review.SubmittedAt,
                review.Url)));

        return new ContributionDetailDto(
            pullRequest.Id,
            "pull_request",
            pullRequest.RepositoryId,
            pullRequest.RepositoryFullName,
            pullRequest.Number.ToString(),
            pullRequest.Title,
            pullRequest.AuthorLogin,
            pullRequest.CreatedAtGitHub,
            status,
            pullRequest.Url,
            pullRequest.IsApproved,
            evidence);
    }
}
