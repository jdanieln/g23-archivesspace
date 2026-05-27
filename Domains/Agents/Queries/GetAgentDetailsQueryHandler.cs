using System.Threading.Tasks;
using ArchivesSpaceWeb.Domains.Agents.Interfaces;
using ArchivesSpaceWeb.Domains.Shared.Interfaces;

namespace ArchivesSpaceWeb.Domains.Agents.Queries
{
    public class GetAgentDetailsQueryHandler : IQueryHandler<GetAgentDetailsQuery, AgentDetailsResult?>
    {
        private readonly IAgentRepository _agentRepository;

        public GetAgentDetailsQueryHandler(IAgentRepository agentRepository)
        {
            _agentRepository = agentRepository;
        }

        public async Task<AgentDetailsResult?> HandleAsync(GetAgentDetailsQuery query)
        {
            var agent = await _agentRepository.GetByIdAsync(query.Id);
            if (agent == null) return null;

            var resources = await _agentRepository.GetLinkedResourcesAsync(query.Id);
            var accessions = await _agentRepository.GetLinkedAccessionsAsync(query.Id);

            return new AgentDetailsResult
            {
                Agent = agent,
                Resources = resources,
                Accessions = accessions
            };
        }
    }
}
