using System.Threading;
using System.Threading.Tasks;
using CodeImpact.Application.Common.Interfaces;
using CodeImpact.Application.GitHub.Commands;
using CodeImpact.Domain.Repositories;
using MediatR;

namespace CodeImpact.Application.GitHub.Commands
{
    public class SyncGitHubRepositoryCommandHandler : IRequestHandler<SyncGitHubRepositoryCommand>
    {
        private readonly IGitHubAccountRepository _gitHubAccountRepository;
        private readonly IGitHubRepositorySelectionRepository _selectionRepository;
        private readonly IGitHubService _gitHubService;

        public SyncGitHubRepositoryCommandHandler(
            IGitHubAccountRepository gitHubAccountRepository,
            IGitHubRepositorySelectionRepository selectionRepository,
            IGitHubService gitHubService)
        {
            _gitHubAccountRepository = gitHubAccountRepository;
            _selectionRepository = selectionRepository;
            _gitHubService = gitHubService;
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

            // Fase 3: sincronização busca PRs da API do GitHub sem persistência detalhada.
            _ = await _gitHubService.GetPullRequestsAsync(account.EncryptedAccessToken, selection.FullName);
            return Unit.Value;
        }
    }
}
