using System.Threading;
using System.Threading.Tasks;
using CodeImpact.Application.Common.Interfaces;
using CodeImpact.Application.GitHub.Commands;
using CodeImpact.Application.GitHub.Dto;
using CodeImpact.Domain.Repositories;
using CodeImpact.Domain.Entities;
using MediatR;

namespace CodeImpact.Application.GitHub.Commands
{
    public class LinkGitHubAccountCommandHandler : IRequestHandler<LinkGitHubAccountCommand, GitHubAccountDto>
    {
        private readonly IGitHubService _gitHubService;
        private readonly IGitHubAccountRepository _gitHubAccountRepository;

        public LinkGitHubAccountCommandHandler(
            IGitHubService gitHubService,
            IGitHubAccountRepository gitHubAccountRepository)
        {
            _gitHubService = gitHubService;
            _gitHubAccountRepository = gitHubAccountRepository;
        }

        public async Task<GitHubAccountDto> Handle(LinkGitHubAccountCommand request, CancellationToken cancellationToken)
        {
            var exchange = await _gitHubService.ExchangeCodeAsync(request.Code);

            var existing = await _gitHubAccountRepository.GetByGitHubUserIdAsync(exchange.GitHubUserId);
            if (existing is not null && existing.UserId != request.UserId)
            {
                throw new InvalidOperationException("Este GitHub já está vinculado a outro usuário.");
            }

            if (existing is null)
            {
                var githubAccount = new GitHubAccount(
                    request.UserId,
                    exchange.GitHubUsername,
                    exchange.GitHubUserId,
                    exchange.EncryptedAccessToken);

                await _gitHubAccountRepository.AddAsync(githubAccount);
                return new GitHubAccountDto(githubAccount.Id, githubAccount.GitHubUsername, githubAccount.GitHubUserId, githubAccount.LinkedAt);
            }

            existing.UpdateAccessToken(exchange.EncryptedAccessToken);
            await _gitHubAccountRepository.UpdateAsync(existing);

            return new GitHubAccountDto(existing.Id, existing.GitHubUsername, existing.GitHubUserId, existing.LinkedAt);
        }
    }
}
