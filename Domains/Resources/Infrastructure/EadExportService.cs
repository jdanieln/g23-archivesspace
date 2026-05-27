using ArchivesSpaceWeb.Domains.Resources.Entities;
using ArchivesSpaceWeb.Domains.Resources.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace ArchivesSpaceWeb.Domains.Resources.Infrastructure
{
    public class EadExportService : IEadExportService
    {
        public XDocument GenerateEadXml(Resource resource, List<ArchivalObject> components)
        {
            var doc = new XDocument(
                new XElement("ead",
                    new XAttribute("audience", "external"),
                    new XElement("eadheader",
                        new XElement("eadid", resource.Identifier),
                        new XElement("filedesc",
                            new XElement("titlestmt",
                                new XElement("titleproper", resource.Title)
                            ),
                            new XElement("publicationstmt",
                                new XElement("publisher", resource.Repository?.Name ?? "ArchivesSpaceWeb Repository"),
                                new XElement("p", $"Author: {resource.FindingAidAuthor ?? "Archivist Staff"}")
                            )
                        )
                    ),
                    new XElement("archdesc", new XAttribute("level", resource.LevelOfDescription.ToLower()),
                        new XElement("did",
                            new XElement("unittitle", resource.Title),
                            new XElement("unitid", resource.Identifier),
                            new XElement("unitdate", resource.Dates),
                            new XElement("physdesc",
                                new XElement("extent", resource.Extents)
                            ),
                            new XElement("langmaterial",
                                new XElement("language", resource.LanguageOfDescription ?? "Spanish")
                            )
                        ),
                        new XElement("dsc",
                            BuildXmlHierarchy(null, components)
                        )
                    )
                )
            );

            return doc;
        }

        private List<XElement> BuildXmlHierarchy(int? parentId, List<ArchivalObject> list)
        {
            var elements = new List<XElement>();
            var levelChildren = list.Where(c => c.ParentId == parentId).OrderBy(c => c.Position);

            foreach (var child in levelChildren)
            {
                var tag = parentId == null ? "c01" : "c"; // standard nested EAD component tag
                var childElem = new XElement(tag,
                    new XAttribute("level", child.LevelOfDescription.ToLower()),
                    new XAttribute("id", child.ComponentIdentifier ?? $"comp_{child.Id}"),
                    new XElement("did",
                        new XElement("unittitle", child.Title),
                        new XElement("unitid", child.ComponentIdentifier),
                        new XElement("unitdate", child.Dates),
                        new XElement("physdesc",
                            new XElement("extent", child.Extents)
                        )
                    )
                );

                var grandchildren = BuildXmlHierarchy(child.Id, list);
                if (grandchildren.Any())
                {
                    childElem.Add(grandchildren);
                }

                elements.Add(childElem);
            }

            return elements;
        }
    }
}
