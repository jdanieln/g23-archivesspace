using System.Threading.Tasks;
using ArchivesSpaceWeb.Domains.Agents.Entities;
using ArchivesSpaceWeb.Domains.Agents.Interfaces;
using ArchivesSpaceWeb.Domains.Admin.Interfaces;
using ArchivesSpaceWeb.Domains.Shared.Interfaces;

namespace ArchivesSpaceWeb.Domains.Agents.Commands
{
    public class EditAgentCommandHandler : ICommandHandler<EditAgentCommand, Agent>
    {
        private readonly IAgentRepository _agentRepository;
        private readonly IAuditService _auditService;

        public EditAgentCommandHandler(IAgentRepository agentRepository, IAuditService auditService)
        {
            _agentRepository = agentRepository;
            _auditService = auditService;
        }

        public async Task<Agent> HandleAsync(EditAgentCommand command)
        {
            await _agentRepository.UpdateAsync(command.Agent);
            await _auditService.LogEventAsync("Modification", $"Agente '{command.Agent.Name}' modificado por {command.ModifiedBy ?? "Sistema"}.", agentId: command.Agent.Id);
            await _agentRepository.SaveChangesAsync();

            return command.Agent;
        }
    }
}
