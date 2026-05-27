using System.Threading.Tasks;
using ArchivesSpaceWeb.Domains.Resources.Interfaces;
using ArchivesSpaceWeb.Domains.Shared.Interfaces;

namespace ArchivesSpaceWeb.Domains.Resources.Commands
{
    public class UpdateResourceHierarchyCommandHandler : ICommandHandler<UpdateResourceHierarchyCommand, bool>
    {
        private readonly IResourceRepository _resourceRepository;

        public UpdateResourceHierarchyCommandHandler(IResourceRepository resourceRepository)
        {
            _resourceRepository = resourceRepository;
        }

        public async Task<bool> HandleAsync(UpdateResourceHierarchyCommand command)
        {
            foreach (var update in command.Updates)
            {
                var component = await _resourceRepository.GetArchivalObjectByIdAsync(update.Id);
                if (component != null)
                {
                    component.ParentId = update.ParentId == 0 ? null : update.ParentId;
                    component.Position = update.Position;
                    await _resourceRepository.UpdateArchivalObjectAsync(component);
                }
            }
            await _resourceRepository.SaveChangesAsync();
            return true;
        }
    }
}
