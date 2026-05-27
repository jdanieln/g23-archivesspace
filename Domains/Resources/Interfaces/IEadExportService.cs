using ArchivesSpaceWeb.Domains.Resources.Entities;
using System.Collections.Generic;
using System.Xml.Linq;

namespace ArchivesSpaceWeb.Domains.Resources.Interfaces
{
    public interface IEadExportService
    {
        XDocument GenerateEadXml(Resource resource, List<ArchivalObject> components);
    }
}
