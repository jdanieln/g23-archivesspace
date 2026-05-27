using System;
using System.Collections.Generic;
using ArchivesSpaceWeb.Domains.Admin.Entities;
using ArchivesSpaceWeb.Domains.Shared.Interfaces;

namespace ArchivesSpaceWeb.Domains.Admin.Queries
{
    public class GetDashboardStatsQuery : IQuery<DashboardStatsResult>
    {
        public string? RepositoryIdClaim { get; }

        public GetDashboardStatsQuery(string? repositoryIdClaim)
        {
            RepositoryIdClaim = repositoryIdClaim;
        }
    }

    public class DashboardStatsResult
    {
        public int RepositoryCount { get; set; }
        public int ResourceCount { get; set; }
        public int AccessionCount { get; set; }
        public int AgentCount { get; set; }
        public int SubjectCount { get; set; }
        public int DigitalObjectCount { get; set; }
        public List<Event> RecentEvents { get; set; } = new();
        public string ActiveRepositoryName { get; set; } = "Ninguno Seleccionado";
    }
}
