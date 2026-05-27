using System.Threading.Tasks;
using ArchivesSpaceWeb.Domains.Admin.Interfaces;
using ArchivesSpaceWeb.Domains.Shared.Interfaces;

namespace ArchivesSpaceWeb.Domains.Admin.Commands
{
    public class ImportEadXmlCommandHandler : ICommandHandler<ImportEadXmlCommand, ImportXmlResult>
    {
        private readonly IImportService _importService;

        public ImportEadXmlCommandHandler(IImportService importService)
        {
            _importService = importService;
        }

        public async Task<ImportXmlResult> HandleAsync(ImportEadXmlCommand command)
        {
            var result = await _importService.ImportEadAsync(command.Stream, command.RepositoryId);
            return new ImportXmlResult
            {
                Success = result.Success,
                Message = result.Message,
                LogDetails = result.LogDetails
            };
        }
    }
}
