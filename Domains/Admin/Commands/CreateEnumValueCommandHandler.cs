using System.Threading.Tasks;
using ArchivesSpaceWeb.Domains.Admin.Interfaces;
using ArchivesSpaceWeb.Domains.Shared.Interfaces;

namespace ArchivesSpaceWeb.Domains.Admin.Commands
{
    public class CreateEnumValueCommandHandler : ICommandHandler<CreateEnumValueCommand, bool>
    {
        private readonly IEnumService _enumService;

        public CreateEnumValueCommandHandler(IEnumService enumService)
        {
            _enumService = enumService;
        }

        public async Task<bool> HandleAsync(CreateEnumValueCommand command)
        {
            await _enumService.AddEnumValueAsync(command.EnumListId, command.Value, command.Label);
            return true;
        }
    }
}
