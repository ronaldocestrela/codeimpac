using CodeImpact.Application.GitHub.Dto;
using CodeImpact.Domain.Repositories;
using MediatR;

namespace CodeImpact.Application.GitHub.Queries;

public sealed class GetCommitContributionDetailQueryHandler : IRequestHandler<GetCommitContributionDetailQuery, ContributionDetailDto?>
{
    private readonly IGitHubCommitRepository _commitRepository;

    public GetCommitContributionDetailQueryHandler(IGitHubCommitRepository commitRepository)
    {
        _commitRepository = commitRepository;
    }

    public async Task<ContributionDetailDto?> Handle(GetCommitContributionDetailQuery request, CancellationToken cancellationToken)
    {
        var commit = await _commitRepository.GetByIdAsync(request.UserId, request.ContributionId);
        if (commit is null)
        {
            return null;
        }

        return new ContributionDetailDto(
            commit.Id,
            "commit",
            commit.RepositoryId,
            commit.RepositoryFullName,
            commit.CommitSha,
            commit.Message,
            commit.AuthorName,
            commit.CommittedAt,
            "committed",
            commit.Url,
            null,
            new[]
            {
                new ContributionEvidenceDto(
                    "commit",
                    commit.CommitSha,
                    commit.AuthorName,
                    "committed",
                    commit.CommittedAt,
                    commit.Url)
            });
    }
}
