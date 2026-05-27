using System.Threading.Tasks;
using ArchivesSpaceWeb.Domains.Agents.Entities;
using ArchivesSpaceWeb.Domains.Agents.Interfaces;
using ArchivesSpaceWeb.Domains.Admin.Interfaces;
using ArchivesSpaceWeb.Domains.Shared.Interfaces;

namespace ArchivesSpaceWeb.Domains.Agents.Commands
{
    public class CreateAgentCommandHandler : ICommandHandler<CreateAgentCommand, Agent>
    {
        private readonly IAgentRepository _agentRepository;
        private readonly IAuditService _auditService;

        public CreateAgentCommandHandler(IAgentRepository agentRepository, IAuditService auditService)
        {
            _agentRepository = agentRepository;
            _auditService = auditService;
        }

        public async Task<Agent> HandleAsync(CreateAgentCommand command)
        {
            await _agentRepository.AddAsync(command.Agent);
            await _agentRepository.SaveChangesAsync();

            // Log creation event (US 37)
            await _auditService.LogEventAsync("Creation", $"Agente '{command.Agent.Name}' ({command.Agent.Type}) registrado por {command.CreatedBy ?? "Sistema"}.", agentId: command.Agent.Id);

            return command.Agent;
        }
    }
}
