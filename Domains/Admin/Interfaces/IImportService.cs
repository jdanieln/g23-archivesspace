using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using ArchivesSpaceWeb.Domains.Admin.Entities;

namespace ArchivesSpaceWeb.Domains.Admin.Interfaces
{
    public interface IImportService
    {
        Task<List<ImportLog>> GetRecentImportLogsAsync(int count);
        Task<List<Repository>> GetAllRepositoriesAsync();
        Task<(bool Success, string Message, string LogDetails)> ImportEadAsync(Stream xmlStream, int repositoryId);
        Task<(bool Success, string Message, string LogDetails)> ImportMarcXmlAsync(Stream xmlStream, int repositoryId, bool agentsAndSubjectsOnly);
        Task<(bool Success, string Message, string LogDetails)> ImportEacCpfAsync(Stream xmlStream);
        Task<(bool Success, int SuccessCount, int ErrorCount, string LogDetails)> ImportAccessionsCsvAsync(Stream csvStream, int repositoryId);
    }
}
