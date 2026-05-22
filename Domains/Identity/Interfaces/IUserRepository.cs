using System.Collections.Generic;
using System.Threading.Tasks;
using ArchivesSpaceWeb.Domains.Identity.Entities;
using ArchivesSpaceWeb.Domains.Shared.Interfaces;

namespace ArchivesSpaceWeb.Domains.Identity.Interfaces
{
    public interface IUserRepository : IRepository<User>
    {
        Task<List<User>> GetUsersWithRepositoryAsync();
    }
}
