using ArchivesSpaceWeb.Domains.Agents.Entities;
using ArchivesSpaceWeb.Domains.Agents.Interfaces;
using System.Xml.Linq;

namespace ArchivesSpaceWeb.Domains.Agents.Infrastructure
{
    public class EacCpfExportService : IEacCpfExportService
    {
        public XDocument GenerateEacCpfXml(Agent agent)
        {
            var doc = new XDocument(
                new XElement("eac-cpf",
                    new XAttribute("xmlns", "urn:isbn:1-931666-33-4"),
                    new XElement("control",
                        new XElement("recordId", $"agent-{agent.Id}"),
                        new XElement("maintenanceStatus", "derived"),
                        new XElement("maintenanceAgency",
                            new XElement("agencyName", "ArchivesSpaceWeb G23 System")
                        ),
                        new XElement("languageDeclaration",
                            new XElement("language", new XAttribute("languageCode", "spa"), "Español")
                        )
                    ),
                    new XElement("identity",
                        new XElement("entityType", agent.Type.ToLower() == "corporate" ? "corporateBody" : agent.Type.ToLower()),
                        new XElement("nameEntry",
                            new XElement("part", agent.Name),
                            new XElement("authorizedForm", "local")
                        )
                    ),
                    new XElement("description",
                        new XElement("existDates",
                            new XElement("dateRange",
                                new XElement("fromDate", "N/D"),
                                new XElement("toDate", "N/D")
                            )
                        ),
                        new XElement("biogHist",
                            new XElement("p", $"Registro de agente importado/administrado como {agent.Type}. Autoridad origen: {agent.Source ?? "Local"}.")
                        )
                    )
                )
            );

            return doc;
        }
    }
}
