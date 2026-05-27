using System.Collections.Generic;
using ArchivesSpaceWeb.Domains.Resources.Entities;
using ArchivesSpaceWeb.Domains.Shared.Interfaces;

namespace ArchivesSpaceWeb.Domains.Resources.Queries
{
    public class SearchFindingAidsQuery : IQuery<List<Resource>>
    {
        public string? Search { get; }

        public SearchFindingAidsQuery(string? search)
        {
            Search = search;
        }
    }
}
