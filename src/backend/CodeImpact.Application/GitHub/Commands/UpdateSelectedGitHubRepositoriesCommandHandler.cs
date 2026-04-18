using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CodeImpact.Domain.Entities;
using CodeImpact.Domain.Repositories;
using MediatR;

namespace CodeImpact.Application.GitHub.Commands
{
    public class UpdateSelectedGitHubRepositoriesCommandHandler : IRequestHandler<UpdateSelectedGitHubRepositoriesCommand>
    {
        private readonly IGitHubAccountRepository _gitHubAccountRepository;
        private readonly IGitHubRepositorySelectionRepository _selectionRepository;

        public UpdateSelectedGitHubRepositoriesCommandHandler(
            IGitHubAccountRepository gitHubAccountRepository,
            IGitHubRepositorySelectionRepository selectionRepository)
        {
            _gitHubAccountRepository = gitHubAccountRepository;
            _selectionRepository = selectionRepository;
        }

        public async Task<Unit> Handle(UpdateSelectedGitHubRepositoriesCommand request, CancellationToken cancellationToken)
        {
            var account = await _gitHubAccountRepository.GetByUserIdAsync(request.UserId);
            if (account is null)
            {
                throw new InvalidOperationException("Conta GitHub não encontrada para este usuário.");
            }

            var selections = request.Repositories
                .Select(repo => new GitHubRepositorySelection(
                    request.UserId,
                    account.Id,
                    repo.Id,
                    repo.Name,
                    repo.FullName,
                    repo.Private))
                .ToList();

            await _selectionRepository.ReplaceForUserAsync(request.UserId, account.Id, selections);
            return Unit.Value;
        }
    }
}
