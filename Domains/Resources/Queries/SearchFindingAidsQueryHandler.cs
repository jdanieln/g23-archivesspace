using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ArchivesSpaceWeb.Domains.Resources.Entities;
using ArchivesSpaceWeb.Domains.Resources.Interfaces;
using ArchivesSpaceWeb.Domains.Shared.Interfaces;

namespace ArchivesSpaceWeb.Domains.Resources.Queries
{
    public class SearchFindingAidsQueryHandler : IQueryHandler<SearchFindingAidsQuery, List<Resource>>
    {
        private readonly IResourceRepository _resourceRepository;

        public SearchFindingAidsQueryHandler(IResourceRepository resourceRepository)
        {
            _resourceRepository = resourceRepository;
        }

        public async Task<List<Resource>> HandleAsync(SearchFindingAidsQuery query)
        {
            var resources = await _resourceRepository.GetAllWithRepositoryAsync();
            
            if (!string.IsNullOrEmpty(query.Search))
            {
                var search = query.Search.ToLower();
                resources = resources.Where(r => r.Title.ToLower().Contains(search) || r.Identifier.ToLower().Contains(search)).ToList();
            }

            return resources;
        }
    }
}
