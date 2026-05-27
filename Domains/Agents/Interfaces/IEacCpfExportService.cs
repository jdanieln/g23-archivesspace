using ArchivesSpaceWeb.Domains.Agents.Entities;
using System.Xml.Linq;

namespace ArchivesSpaceWeb.Domains.Agents.Interfaces
{
    public interface IEacCpfExportService
    {
        XDocument GenerateEacCpfXml(Agent agent);
    }
}
