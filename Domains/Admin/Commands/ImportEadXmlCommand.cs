using System.IO;
using ArchivesSpaceWeb.Domains.Shared.Interfaces;

namespace ArchivesSpaceWeb.Domains.Admin.Commands
{
    public class ImportEadXmlCommand : ICommand<ImportXmlResult>
    {
        public Stream Stream { get; }
        public int RepositoryId { get; }

        public ImportEadXmlCommand(Stream stream, int repositoryId)
        {
            Stream = stream;
            RepositoryId = repositoryId;
        }
    }

    public class ImportXmlResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string LogDetails { get; set; } = string.Empty;
    }
}
