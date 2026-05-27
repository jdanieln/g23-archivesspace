using System.Collections.Generic;
using System.Threading.Tasks;
using ArchivesSpaceWeb.Domains.Resources.Entities;
using ArchivesSpaceWeb.Domains.Resources.Interfaces;
using ArchivesSpaceWeb.Domains.Shared.Interfaces;

namespace ArchivesSpaceWeb.Domains.Resources.Queries
{
    public class GetResourceHierarchyQueryHandler : IQueryHandler<GetResourceHierarchyQuery, List<ArchivalObject>>
    {
        private readonly IResourceRepository _resourceRepository;

        public GetResourceHierarchyQueryHandler(IResourceRepository resourceRepository)
        {
            _resourceRepository = resourceRepository;
        }

        public async Task<List<ArchivalObject>> HandleAsync(GetResourceHierarchyQuery query)
        {
            return await _resourceRepository.GetComponentsTreeAsync(query.ResourceId);
        }
    }
}
