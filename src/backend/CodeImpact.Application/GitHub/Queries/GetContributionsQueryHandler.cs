using CodeImpact.Application.GitHub.Dto;
using CodeImpact.Domain.Repositories;
using MediatR;

namespace CodeImpact.Application.GitHub.Queries;

public sealed class GetContributionsQueryHandler : IRequestHandler<GetContributionsQuery, PagedContributionsDto>
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

    public async Task<PagedContributionsDto> Handle(GetContributionsQuery request, CancellationToken cancellationToken)
    {
        if (request.From.HasValue && request.To.HasValue && request.From > request.To)
        {
            throw new InvalidOperationException("Período inválido: 'from' deve ser menor ou igual a 'to'.");
        }

        if (request.Page <= 0)
        {
            throw new InvalidOperationException("Pagina invalida: 'page' deve ser maior que zero.");
        }

        if (request.PageSize <= 0 || request.PageSize > 100)
        {
            throw new InvalidOperationException("Tamanho de pagina invalido: 'pageSize' deve estar entre 1 e 100.");
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

        var totalCount = items.Count;
        var totalPages = totalCount == 0
            ? 0
            : (int)Math.Ceiling(totalCount / (double)request.PageSize);

        var safePage = totalPages == 0
            ? 1
            : Math.Min(request.Page, totalPages);

        var pagedItems = items
            .Skip((safePage - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        return new PagedContributionsDto(
            pagedItems,
            totalCount,
            commits.Count,
            pullRequests.Count,
            pullRequests.Count(pr => pr.IsApproved),
            safePage,
            request.PageSize,
            totalPages);
    }
}
