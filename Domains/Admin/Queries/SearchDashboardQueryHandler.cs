using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ArchivesSpaceWeb.Domains.Resources.Entities;
using ArchivesSpaceWeb.Domains.Resources.Interfaces;
using ArchivesSpaceWeb.Domains.Accessions.Entities;
using ArchivesSpaceWeb.Domains.Accessions.Interfaces;
using ArchivesSpaceWeb.Domains.Admin.Entities;
using ArchivesSpaceWeb.Domains.Shared.Interfaces;

namespace ArchivesSpaceWeb.Domains.Admin.Queries
{
    public class SearchDashboardQueryHandler : IQueryHandler<SearchDashboardQuery, SearchDashboardResult>
    {
        private readonly IResourceRepository _resourceRepository;
        private readonly IAccessionRepository _accessionRepository;
        private readonly IRepository<DigitalObject> _digitalObjectRepo;

        public SearchDashboardQueryHandler(
            IResourceRepository resourceRepository,
            IAccessionRepository accessionRepository,
            IRepository<DigitalObject> digitalObjectRepo)
        {
            _resourceRepository = resourceRepository;
            _accessionRepository = accessionRepository;
            _digitalObjectRepo = digitalObjectRepo;
        }

        public async Task<SearchDashboardResult> HandleAsync(SearchDashboardQuery query)
        {
            if (string.IsNullOrEmpty(query.Query))
            {
                return new SearchDashboardResult();
            }

            var searchQuery = query.Query.ToLower().Trim();

            var allResources = await _resourceRepository.GetAllWithRepositoryAsync();
            var allAccessions = await _accessionRepository.GetAllWithRepositoryAsync();
            var allDigitalObjects = await _digitalObjectRepo.GetAllAsync();

            var resourceFilter = allResources.AsQueryable();
            var accessionFilter = allAccessions.AsQueryable();
            var digitalObjectFilter = allDigitalObjects.AsQueryable();

            if (query.RepositoryId.HasValue && query.RepositoryId > 0)
            {
                resourceFilter = resourceFilter.Where(r => r.RepositoryId == query.RepositoryId.Value);
                accessionFilter = accessionFilter.Where(a => a.RepositoryId == query.RepositoryId.Value);
                digitalObjectFilter = digitalObjectFilter.Where(d => d.RepositoryId == query.RepositoryId.Value);
            }

            var resourcesResult = resourceFilter
                .Where(r => r.Title.ToLower().Contains(searchQuery) || r.Identifier.ToLower().Contains(searchQuery) || (r.Dates != null && r.Dates.ToLower().Contains(searchQuery)))
                .ToList();

            var accessionsResult = accessionFilter
                .Where(a => a.Title.ToLower().Contains(searchQuery) || a.Identifier.ToLower().Contains(searchQuery) || (a.Dates != null && a.Dates.ToLower().Contains(searchQuery)))
                .ToList();

            var digitalObjectsResult = digitalObjectFilter
                .Where(d => d.Title.ToLower().Contains(searchQuery) || d.Identifier.ToLower().Contains(searchQuery) || d.FileUri.ToLower().Contains(searchQuery))
                .ToList();

            return new SearchDashboardResult
            {
                Resources = resourcesResult,
                Accessions = accessionsResult,
                DigitalObjects = digitalObjectsResult
            };
        }
    }
}
