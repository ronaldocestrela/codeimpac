using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CodeImpact.Application.Common.Interfaces;
using CodeImpact.Application.GitHub.Dto;
using CodeImpact.Application.GitHub.Queries;
using CodeImpact.Domain.Repositories;
using MediatR;

namespace CodeImpact.Application.GitHub.Queries
{
    public class GetGitHubRepositoriesQueryHandler : IRequestHandler<GetGitHubRepositoriesQuery, IEnumerable<GitHubRepositoryDto>>
    {
        private readonly IGitHubService _gitHubService;
        private readonly IGitHubAccountRepository _gitHubAccountRepository;

        public GetGitHubRepositoriesQueryHandler(
            IGitHubService gitHubService,
            IGitHubAccountRepository gitHubAccountRepository)
        {
            _gitHubService = gitHubService;
            _gitHubAccountRepository = gitHubAccountRepository;
        }

        public async Task<IEnumerable<GitHubRepositoryDto>> Handle(GetGitHubRepositoriesQuery request, CancellationToken cancellationToken)
        {
            var account = await _gitHubAccountRepository.GetByUserIdAsync(request.UserId);
            if (account is null)
            {
                return Enumerable.Empty<GitHubRepositoryDto>();
            }

            return await _gitHubService.GetUserRepositoriesAsync(account.EncryptedAccessToken);
        }
    }
}
