using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ArchivesSpaceWeb.Domains.Accessions.Entities;
using ArchivesSpaceWeb.Domains.Accessions.Interfaces;
using ArchivesSpaceWeb.Domains.Shared.Infrastructure;
using ArchivesSpaceWeb.Domains.Admin.Entities;

namespace ArchivesSpaceWeb.Domains.Accessions.Infrastructure
{
    public class EFAccessionRepository : EFGenericRepository<Accession>, IAccessionRepository
    {
        public EFAccessionRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<Accession?> GetAccessionWithDetailsAsync(int id)
        {
            return await _context.Accessions
                .Include(a => a.Repository)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<List<Accession>> GetAllWithRepositoryAsync()
        {
            return await _context.Accessions
                .Include(a => a.Repository)
                .ToListAsync();
        }

        public async Task<List<Accession>> GetByRepositoryAsync(int repositoryId)
        {
            return await _context.Accessions
                .Include(a => a.Repository)
                .Where(a => a.RepositoryId == repositoryId)
                .ToListAsync();
        }

        public async Task<List<object>> GetAccessionAgentsAsync(int accessionId)
        {
            var list = await _context.AccessionAgents
                .Include(aa => aa.Agent)
                .Where(aa => aa.AccessionId == accessionId)
                .ToListAsync();

            return list.Select(aa => (object)new { aa.Agent, aa.Role }).ToList();
        }

        public async Task<List<Repository>> GetAllRepositoriesAsync()
        {
            return await _context.Repositories.ToListAsync();
        }
    }
}
