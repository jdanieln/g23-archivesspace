using ArchivesSpaceWeb.Domains.Agents.Entities;
using ArchivesSpaceWeb.Domains.Shared.Interfaces;

namespace ArchivesSpaceWeb.Domains.Agents.Commands
{
    public class EditAgentCommand : ICommand<Agent>
    {
        public Agent Agent { get; }
        public string? ModifiedBy { get; }

        public EditAgentCommand(Agent agent, string? modifiedBy)
        {
            Agent = agent;
            ModifiedBy = modifiedBy;
        }
    }
}
