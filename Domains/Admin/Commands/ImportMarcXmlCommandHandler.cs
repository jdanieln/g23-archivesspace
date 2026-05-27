using System.Threading.Tasks;
using ArchivesSpaceWeb.Domains.Admin.Interfaces;
using ArchivesSpaceWeb.Domains.Shared.Interfaces;

namespace ArchivesSpaceWeb.Domains.Admin.Commands
{
    public class ImportMarcXmlCommandHandler : ICommandHandler<ImportMarcXmlCommand, ImportXmlResult>
    {
        private readonly IImportService _importService;

        public ImportMarcXmlCommandHandler(IImportService importService)
        {
            _importService = importService;
        }

        public async Task<ImportXmlResult> HandleAsync(ImportMarcXmlCommand command)
        {
            var result = await _importService.ImportMarcXmlAsync(command.Stream, command.RepositoryId, command.AgentsAndSubjectsOnly);
            return new ImportXmlResult
            {
                Success = result.Success,
                Message = result.Message,
                LogDetails = result.LogDetails
            };
        }
    }
}
