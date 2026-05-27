using System.Threading.Tasks;
using ArchivesSpaceWeb.Domains.Admin.Interfaces;
using ArchivesSpaceWeb.Domains.Shared.Interfaces;

namespace ArchivesSpaceWeb.Domains.Admin.Commands
{
    public class BulkUpdateEnumCommandHandler : ICommandHandler<BulkUpdateEnumCommand, int>
    {
        private readonly IEnumService _enumService;

        public BulkUpdateEnumCommandHandler(IEnumService enumService)
        {
            _enumService = enumService;
        }

        public async Task<int> HandleAsync(BulkUpdateEnumCommand command)
        {
            return await _enumService.BulkUpdateEnumValuesAsync(command.EnumListId, command.OldValue, command.NewValue);
        }
    }
}
