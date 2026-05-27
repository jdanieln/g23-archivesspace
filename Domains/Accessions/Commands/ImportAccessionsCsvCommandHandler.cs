using System.Threading.Tasks;
using ArchivesSpaceWeb.Domains.Admin.Interfaces;
using ArchivesSpaceWeb.Domains.Shared.Interfaces;

namespace ArchivesSpaceWeb.Domains.Accessions.Commands
{
    public class ImportAccessionsCsvCommandHandler : ICommandHandler<ImportAccessionsCsvCommand, ImportAccessionsCsvResult>
    {
        private readonly IImportService _importService;

        public ImportAccessionsCsvCommandHandler(IImportService importService)
        {
            _importService = importService;
        }

        public async Task<ImportAccessionsCsvResult> HandleAsync(ImportAccessionsCsvCommand command)
        {
            var result = await _importService.ImportAccessionsCsvAsync(command.Stream, command.RepositoryId);
            return new ImportAccessionsCsvResult
            {
                Success = result.Success,
                SuccessCount = result.SuccessCount,
                ErrorCount = result.ErrorCount,
                LogDetails = result.LogDetails
            };
        }
    }
}
