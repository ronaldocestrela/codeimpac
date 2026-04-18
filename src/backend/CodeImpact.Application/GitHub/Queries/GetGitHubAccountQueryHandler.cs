using CodeImpact.Application.GitHub.Dto;
using CodeImpact.Domain.Repositories;
using MediatR;

namespace CodeImpact.Application.GitHub.Queries;

public sealed class GetGitHubAccountQueryHandler : IRequestHandler<GetGitHubAccountQuery, GitHubAccountDto?>
{
    private readonly IGitHubAccountRepository _gitHubAccountRepository;

    public GetGitHubAccountQueryHandler(IGitHubAccountRepository gitHubAccountRepository)
    {
        _gitHubAccountRepository = gitHubAccountRepository;
    }

    public async Task<GitHubAccountDto?> Handle(GetGitHubAccountQuery request, CancellationToken cancellationToken)
    {
        var account = await _gitHubAccountRepository.GetByUserIdAsync(request.UserId);
        return account is null
            ? null
            : new GitHubAccountDto(account.Id, account.GitHubUsername, account.GitHubUserId, account.LinkedAt);
    }
}
