using System.Threading.Tasks;
using ArchivesSpaceWeb.Domains.Resources.Interfaces;
using ArchivesSpaceWeb.Domains.Shared.Interfaces;

namespace ArchivesSpaceWeb.Domains.Resources.Queries
{
    public class GetResourceDetailsQueryHandler : IQueryHandler<GetResourceDetailsQuery, ResourceDetailsResult?>
    {
        private readonly IResourceRepository _resourceRepository;

        public GetResourceDetailsQueryHandler(IResourceRepository resourceRepository)
        {
            _resourceRepository = resourceRepository;
        }

        public async Task<ResourceDetailsResult?> HandleAsync(GetResourceDetailsQuery query)
        {
            var resource = await _resourceRepository.GetResourceWithDetailsAsync(query.Id);
            if (resource == null) return null;

            var components = await _resourceRepository.GetComponentsTreeAsync(query.Id);
            var subjects = await _resourceRepository.GetResourceSubjectsAsync(query.Id);
            var agents = await _resourceRepository.GetResourceAgentsAsync(query.Id);
            var collMgmt = await _resourceRepository.GetCollectionManagementAsync(query.Id);
            var rights = await _resourceRepository.GetRightsStatementsAsync(query.Id);

            return new ResourceDetailsResult
            {
                Resource = resource,
                Components = components,
                Subjects = subjects,
                Agents = agents,
                CollectionManagement = collMgmt,
                RightsStatements = rights
            };
        }
    }
}
