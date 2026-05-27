using System.Collections.Generic;
using ArchivesSpaceWeb.Domains.Resources.Entities;
using ArchivesSpaceWeb.Domains.Admin.Entities;
using ArchivesSpaceWeb.Domains.Shared.Interfaces;

namespace ArchivesSpaceWeb.Domains.Resources.Queries
{
    public class GetResourceDetailsQuery : IQuery<ResourceDetailsResult?>
    {
        public int Id { get; }

        public GetResourceDetailsQuery(int id)
        {
            Id = id;
        }
    }

    public class ResourceDetailsResult
    {
        public Resource Resource { get; set; } = null!;
        public List<ArchivalObject> Components { get; set; } = new();
        public List<Subject> Subjects { get; set; } = new();
        public List<object> Agents { get; set; } = new();
        public CollectionManagement? CollectionManagement { get; set; }
        public List<RightsStatement> RightsStatements { get; set; } = new();
    }
}
