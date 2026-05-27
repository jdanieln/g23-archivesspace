using ArchivesSpaceWeb.Domains.Resources.Entities;
using ArchivesSpaceWeb.Domains.Shared.Interfaces;

namespace ArchivesSpaceWeb.Domains.Resources.Commands
{
    public class CreateResourceCommand : ICommand<Resource>
    {
        public Resource Resource { get; }
        public string? CreatedBy { get; }

        public CreateResourceCommand(Resource resource, string? createdBy)
        {
            Resource = resource;
            CreatedBy = createdBy;
        }
    }
}
