using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ArchivesSpaceWeb.Domains.Agents.Entities;
using ArchivesSpaceWeb.Domains.Agents.Interfaces;
using ArchivesSpaceWeb.Domains.Shared.Infrastructure;
using ArchivesSpaceWeb.Domains.Resources.Entities;
using ArchivesSpaceWeb.Domains.Accessions.Entities;

namespace ArchivesSpaceWeb.Domains.Agents.Infrastructure
{
    public class EFAgentRepository : EFGenericRepository<Agent>, IAgentRepository
    {
        public EFAgentRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<List<ResourceAgent>> GetLinkedResourcesAsync(int agentId)
        {
            return await _context.ResourceAgents
                .Include(ra => ra.Resource)
                .Where(ra => ra.AgentId == agentId)
                .ToListAsync();
        }

        public async Task<List<AccessionAgent>> GetLinkedAccessionsAsync(int agentId)
        {
            return await _context.AccessionAgents
                .Include(aa => aa.Accession)
                .Where(aa => aa.AgentId == agentId)
                .ToListAsync();
        }
    }
}
