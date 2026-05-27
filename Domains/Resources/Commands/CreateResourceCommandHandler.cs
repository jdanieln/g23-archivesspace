using System.Threading.Tasks;
using ArchivesSpaceWeb.Domains.Resources.Entities;
using ArchivesSpaceWeb.Domains.Resources.Interfaces;
using ArchivesSpaceWeb.Domains.Admin.Interfaces;
using ArchivesSpaceWeb.Domains.Shared.Interfaces;

namespace ArchivesSpaceWeb.Domains.Resources.Commands
{
    public class CreateResourceCommandHandler : ICommandHandler<CreateResourceCommand, Resource>
    {
        private readonly IResourceRepository _resourceRepository;
        private readonly IAuditService _auditService;

        public CreateResourceCommandHandler(IResourceRepository resourceRepository, IAuditService auditService)
        {
            _resourceRepository = resourceRepository;
            _auditService = auditService;
        }

        public async Task<Resource> HandleAsync(CreateResourceCommand command)
        {
            await _resourceRepository.AddAsync(command.Resource);
            await _resourceRepository.SaveChangesAsync();

            // Log Event (US 37)
            await _auditService.LogEventAsync("Creation", $"Recurso '{command.Resource.Title}' ({command.Resource.Identifier}) creado por {command.CreatedBy ?? "Sistema"}.", resourceId: command.Resource.Id);

            return command.Resource;
        }
    }
}
