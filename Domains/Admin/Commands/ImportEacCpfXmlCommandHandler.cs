using System.Threading.Tasks;
using ArchivesSpaceWeb.Domains.Admin.Interfaces;
using ArchivesSpaceWeb.Domains.Shared.Interfaces;

namespace ArchivesSpaceWeb.Domains.Admin.Commands
{
    public class ImportEacCpfXmlCommandHandler : ICommandHandler<ImportEacCpfXmlCommand, ImportXmlResult>
    {
        private readonly IImportService _importService;

        public ImportEacCpfXmlCommandHandler(IImportService importService)
        {
            _importService = importService;
        }

        public async Task<ImportXmlResult> HandleAsync(ImportEacCpfXmlCommand command)
        {
            var result = await _importService.ImportEacCpfAsync(command.Stream);
            return new ImportXmlResult
            {
                Success = result.Success,
                Message = result.Message,
                LogDetails = result.LogDetails
            };
        }
    }
}
