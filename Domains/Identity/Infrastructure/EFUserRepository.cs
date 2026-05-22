using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ArchivesSpaceWeb.Domains.Identity.Entities;
using ArchivesSpaceWeb.Domains.Identity.Interfaces;
using ArchivesSpaceWeb.Domains.Shared.Infrastructure;

namespace ArchivesSpaceWeb.Domains.Identity.Infrastructure
{
    public class EFUserRepository : EFGenericRepository<User>, IUserRepository
    {
        public EFUserRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<List<User>> GetUsersWithRepositoryAsync()
        {
            return await _context.Users
                .Include(u => u.Repository)
                .ToListAsync();
        }
    }
}
