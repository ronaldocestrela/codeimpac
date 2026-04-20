using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CodeImpact.Application.Common.Interfaces;
using CodeImpact.Application.GitHub.Dto;
using CodeImpact.Domain.Repositories;
using MediatR;

namespace CodeImpact.Application.GitHub.Queries
{
    public sealed class GetGitHubOrganizationsQueryHandler : IRequestHandler<GetGitHubOrganizationsQuery, IEnumerable<GitHubOrganizationDto>>
    {
        private readonly IGitHubService _gitHubService;
        private readonly IGitHubAccountRepository _gitHubAccountRepository;

        public GetGitHubOrganizationsQueryHandler(
            IGitHubService gitHubService,
            IGitHubAccountRepository gitHubAccountRepository)
        {
            _gitHubService = gitHubService;
            _gitHubAccountRepository = gitHubAccountRepository;
        }

        public async Task<IEnumerable<GitHubOrganizationDto>> Handle(GetGitHubOrganizationsQuery request, CancellationToken cancellationToken)
        {
            var account = await _gitHubAccountRepository.GetByUserIdAsync(request.UserId);
            if (account is null)
            {
                return Enumerable.Empty<GitHubOrganizationDto>();
            }

            return await _gitHubService.GetUserOrganizationsAsync(account.EncryptedAccessToken);
        }
    }
}