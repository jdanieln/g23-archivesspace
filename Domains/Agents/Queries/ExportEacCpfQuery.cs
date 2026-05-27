using System.Xml.Linq;
using ArchivesSpaceWeb.Domains.Shared.Interfaces;

namespace ArchivesSpaceWeb.Domains.Agents.Queries
{
    public class ExportEacCpfQuery : IQuery<XDocument?>
    {
        public int Id { get; }

        public ExportEacCpfQuery(int id)
        {
            Id = id;
        }
    }
}
