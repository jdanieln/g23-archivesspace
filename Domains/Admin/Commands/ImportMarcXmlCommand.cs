using System.IO;
using ArchivesSpaceWeb.Domains.Shared.Interfaces;

namespace ArchivesSpaceWeb.Domains.Admin.Commands
{
    public class ImportMarcXmlCommand : ICommand<ImportXmlResult>
    {
        public Stream Stream { get; }
        public int RepositoryId { get; }
        public bool AgentsAndSubjectsOnly { get; }

        public ImportMarcXmlCommand(Stream stream, int repositoryId, bool agentsAndSubjectsOnly)
        {
            Stream = stream;
            RepositoryId = repositoryId;
            AgentsAndSubjectsOnly = agentsAndSubjectsOnly;
        }
    }
}
