using System.Collections.Generic;
using ArchivesSpaceWeb.Domains.Resources.Entities;
using ArchivesSpaceWeb.Domains.Accessions.Entities;
using ArchivesSpaceWeb.Domains.Admin.Entities;
using ArchivesSpaceWeb.Domains.Shared.Interfaces;

namespace ArchivesSpaceWeb.Domains.Admin.Queries
{
    public class SearchDashboardQuery : IQuery<SearchDashboardResult>
    {
        public int? RepositoryId { get; }
        public string Query { get; }

        public SearchDashboardQuery(int? repositoryId, string query)
        {
            RepositoryId = repositoryId;
            Query = query;
        }
    }

    public class SearchDashboardResult
    {
        public List<Resource> Resources { get; set; } = new();
        public List<Accession> Accessions { get; set; } = new();
        public List<DigitalObject> DigitalObjects { get; set; } = new();
    }
}
