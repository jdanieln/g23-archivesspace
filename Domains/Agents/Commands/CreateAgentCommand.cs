using ArchivesSpaceWeb.Domains.Agents.Entities;
using ArchivesSpaceWeb.Domains.Shared.Interfaces;

namespace ArchivesSpaceWeb.Domains.Agents.Commands
{
    public class CreateAgentCommand : ICommand<Agent>
    {
        public Agent Agent { get; }
        public string? CreatedBy { get; }

        public CreateAgentCommand(Agent agent, string? createdBy)
        {
            Agent = agent;
            CreatedBy = createdBy;
        }
    }
}
