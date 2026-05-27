using System.Xml.Linq;
using ArchivesSpaceWeb.Domains.Shared.Interfaces;

namespace ArchivesSpaceWeb.Domains.Resources.Queries
{
    public class ExportEadQuery : IQuery<XDocument?>
    {
        public int Id { get; }

        public ExportEadQuery(int id)
        {
            Id = id;
        }
    }
}
