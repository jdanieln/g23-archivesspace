using System.IO;
using ArchivesSpaceWeb.Domains.Shared.Interfaces;

namespace ArchivesSpaceWeb.Domains.Accessions.Commands
{
    public class ImportAccessionsCsvCommand : ICommand<ImportAccessionsCsvResult>
    {
        public Stream Stream { get; }
        public int RepositoryId { get; }

        public ImportAccessionsCsvCommand(Stream stream, int repositoryId)
        {
            Stream = stream;
            RepositoryId = repositoryId;
        }
    }

    public class ImportAccessionsCsvResult
    {
        public bool Success { get; set; }
        public int SuccessCount { get; set; }
        public int ErrorCount { get; set; }
        public string LogDetails { get; set; } = string.Empty;
    }
}
