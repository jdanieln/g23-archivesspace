using System.Collections.Generic;
using System.Threading.Tasks;
using ArchivesSpaceWeb.Domains.Agents.Entities;
using ArchivesSpaceWeb.Domains.Shared.Interfaces;
using ArchivesSpaceWeb.Domains.Resources.Entities;
using ArchivesSpaceWeb.Domains.Accessions.Entities;

namespace ArchivesSpaceWeb.Domains.Agents.Interfaces
{
    public interface IAgentRepository : IRepository<Agent>
    {
        Task<List<ResourceAgent>> GetLinkedResourcesAsync(int agentId);
        Task<List<AccessionAgent>> GetLinkedAccessionsAsync(int agentId);
    }
}
