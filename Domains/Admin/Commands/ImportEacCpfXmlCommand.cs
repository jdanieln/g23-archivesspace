using System.IO;
using ArchivesSpaceWeb.Domains.Shared.Interfaces;

namespace ArchivesSpaceWeb.Domains.Admin.Commands
{
    public class ImportEacCpfXmlCommand : ICommand<ImportXmlResult>
    {
        public Stream Stream { get; }

        public ImportEacCpfXmlCommand(Stream stream)
        {
            Stream = stream;
        }
    }
}
