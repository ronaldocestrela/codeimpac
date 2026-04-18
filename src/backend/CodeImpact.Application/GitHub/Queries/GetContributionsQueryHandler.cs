using CodeImpact.Application.GitHub.Dto;
using CodeImpact.Domain.Repositories;
using MediatR;

namespace CodeImpact.Application.GitHub.Queries;

public sealed class GetContributionsQueryHandler : IRequestHandler<GetContributionsQuery, IReadOnlyCollection<ContributionListItemDto>>
{
    private readonly IGitHubCommitRepository _commitRepository;
    private readonly IGitHubPullRequestRepository _pullRequestRepository;

    public GetContributionsQueryHandler(
        IGitHubCommitRepository commitRepository,
        IGitHubPullRequestRepository pullRequestRepository)
    {
        _commitRepository = commitRepository;
        _pullRequestRepository = pullRequestRepository;
    }

    public async Task<IReadOnlyCollection<ContributionListItemDto>> Handle(GetContributionsQuery request, CancellationToken cancellationToken)
    {
        if (request.From.HasValue && request.To.HasValue && request.From > request.To)
        {
            throw new InvalidOperationException("Período inválido: 'from' deve ser menor ou igual a 'to'.");
        }

        var commits = await _commitRepository.ListByUserAsync(request.UserId, request.RepositoryId, request.From, request.To);
        var pullRequests = await _pullRequestRepository.ListByUserAsync(request.UserId, request.RepositoryId, request.From, request.To);

        var items = commits
            .Select(commit => new ContributionListItemDto(
                commit.Id,
                "commit",
                commit.RepositoryId,
                commit.RepositoryFullName,
                commit.Message,
                commit.AuthorName,
                commit.CommittedAt,
                "committed",
                commit.Url,
                null))
            .Concat(pullRequests.Select(pullRequest => new ContributionListItemDto(
                pullRequest.Id,
                "pull_request",
                pullRequest.RepositoryId,
                pullRequest.RepositoryFullName,
                pullRequest.Title,
                pullRequest.AuthorLogin,
                pullRequest.CreatedAtGitHub,
                ContributionStatusMapper.BuildPullRequestStatus(pullRequest.IsApproved, pullRequest.MergedAtGitHub, pullRequest.State),
                pullRequest.Url,
                pullRequest.IsApproved)))
            .OrderByDescending(item => item.OccurredAt)
            .ToList();

        return items;
    }
}
