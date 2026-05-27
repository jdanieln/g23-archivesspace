using System.Collections.Generic;
using ArchivesSpaceWeb.Domains.Agents.Entities;
using ArchivesSpaceWeb.Domains.Resources.Entities;
using ArchivesSpaceWeb.Domains.Accessions.Entities;
using ArchivesSpaceWeb.Domains.Shared.Interfaces;

namespace ArchivesSpaceWeb.Domains.Agents.Queries
{
    public class GetAgentDetailsQuery : IQuery<AgentDetailsResult?>
    {
        public int Id { get; }

        public GetAgentDetailsQuery(int id)
        {
            Id = id;
        }
    }

    public class AgentDetailsResult
    {
        public Agent Agent { get; set; } = null!;
        public List<ResourceAgent> Resources { get; set; } = new();
        public List<AccessionAgent> Accessions { get; set; } = new();
    }
}
