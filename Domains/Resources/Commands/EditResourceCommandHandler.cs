using System.Collections.Generic;
using System.Threading.Tasks;
using ArchivesSpaceWeb.Domains.Resources.Entities;
using ArchivesSpaceWeb.Domains.Resources.Interfaces;
using ArchivesSpaceWeb.Domains.Admin.Interfaces;
using ArchivesSpaceWeb.Domains.Shared.Interfaces;

namespace ArchivesSpaceWeb.Domains.Resources.Commands
{
    public class EditResourceCommandHandler : ICommandHandler<EditResourceCommand, Resource>
    {
        private readonly IResourceRepository _resourceRepository;
        private readonly IAuditService _auditService;

        public EditResourceCommandHandler(IResourceRepository resourceRepository, IAuditService auditService)
        {
            _resourceRepository = resourceRepository;
            _auditService = auditService;
        }

        public async Task<Resource> HandleAsync(EditResourceCommand command)
        {
            // US 40: Optimistic locking validation
            await _resourceRepository.UpdateAsync(command.Resource);
            await _resourceRepository.SaveChangesAsync();

            // US 32: Collection management save
            if (command.CollectionManagement != null)
            {
                command.CollectionManagement.ResourceId = command.Id;
                await _resourceRepository.AddOrUpdateCollectionManagementAsync(command.CollectionManagement);
            }

            // US 41: Rights Management save
            await _resourceRepository.UpdateRightsStatementsAsync(command.Id, command.RightsStatements ?? new List<RightsStatement>());

            // Log event (US 37)
            await _auditService.LogEventAsync("Modification", $"Recurso '{command.Resource.Title}' modificado por {command.ModifiedBy ?? "Sistema"}.", resourceId: command.Resource.Id);

            return command.Resource;
        }
    }
}
