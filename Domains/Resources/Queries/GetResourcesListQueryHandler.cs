using System.Collections.Generic;
using System.Threading.Tasks;
using ArchivesSpaceWeb.Domains.Resources.Entities;
using ArchivesSpaceWeb.Domains.Resources.Interfaces;
using ArchivesSpaceWeb.Domains.Shared.Interfaces;

namespace ArchivesSpaceWeb.Domains.Resources.Queries
{
    public class GetResourcesListQueryHandler : IQueryHandler<GetResourcesListQuery, List<Resource>>
    {
        private readonly IResourceRepository _resourceRepository;

        public GetResourcesListQueryHandler(IResourceRepository resourceRepository)
        {
            _resourceRepository = resourceRepository;
        }

        public async Task<List<Resource>> HandleAsync(GetResourcesListQuery query)
        {
            var repClaim = query.User.FindFirst("RepositoryId")?.Value;
            var isSysAdmin = query.User.IsInRole("SystemAdmin");

            if (!isSysAdmin && int.TryParse(repClaim, out int repId) && repId > 0)
            {
                return await _resourceRepository.GetByRepositoryAsync(repId);
            }
            
            return await _resourceRepository.GetAllWithRepositoryAsync();
        }
    }
}
