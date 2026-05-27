using System.Collections.Generic;
using ArchivesSpaceWeb.Domains.Resources.Entities;
using ArchivesSpaceWeb.Domains.Shared.Interfaces;

namespace ArchivesSpaceWeb.Domains.Resources.Queries
{
    public class GetResourceHierarchyQuery : IQuery<List<ArchivalObject>>
    {
        public int ResourceId { get; }

        public GetResourceHierarchyQuery(int resourceId)
        {
            ResourceId = resourceId;
        }
    }
}
