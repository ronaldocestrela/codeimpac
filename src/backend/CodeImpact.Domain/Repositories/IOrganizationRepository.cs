using System;
using System.Threading.Tasks;
using CodeImpact.Domain.Entities;

namespace CodeImpact.Domain.Repositories
{
    public interface IOrganizationRepository
    {
        Task<Organization?> GetByIdAsync(Guid id);

        Task AddAsync(Organization organization);
    }
}
