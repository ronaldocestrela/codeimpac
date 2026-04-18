using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CodeImpact.Application.GitHub.Dto;
using CodeImpact.Domain.Repositories;
using MediatR;

namespace CodeImpact.Application.GitHub.Queries
{
    public class GetSelectedGitHubRepositoriesQueryHandler : IRequestHandler<GetSelectedGitHubRepositoriesQuery, IReadOnlyCollection<SelectedGitHubRepositoryDto>>
    {
        private readonly IGitHubRepositorySelectionRepository _selectionRepository;

        public GetSelectedGitHubRepositoriesQueryHandler(IGitHubRepositorySelectionRepository selectionRepository)
        {
            _selectionRepository = selectionRepository;
        }

        public async Task<IReadOnlyCollection<SelectedGitHubRepositoryDto>> Handle(GetSelectedGitHubRepositoriesQuery request, CancellationToken cancellationToken)
        {
            var selections = await _selectionRepository.GetByUserIdAsync(request.UserId);
            return selections
                .Select(item => new SelectedGitHubRepositoryDto(item.RepositoryId, item.Name, item.FullName, item.Private))
                .ToList();
        }
    }
}
