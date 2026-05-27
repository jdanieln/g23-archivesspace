using System.Collections.Generic;
using System.Threading.Tasks;
using ArchivesSpaceWeb.Domains.Agents.Entities;
using ArchivesSpaceWeb.Domains.Agents.Interfaces;
using ArchivesSpaceWeb.Domains.Shared.Interfaces;

namespace ArchivesSpaceWeb.Domains.Agents.Queries
{
    public class GetAgentsListQueryHandler : IQueryHandler<GetAgentsListQuery, List<Agent>>
    {
        private readonly IAgentRepository _agentRepository;

        public GetAgentsListQueryHandler(IAgentRepository agentRepository)
        {
            _agentRepository = agentRepository;
        }

        public async Task<List<Agent>> HandleAsync(GetAgentsListQuery query)
        {
            return await _agentRepository.GetAllAsync();
        }
    }
}
