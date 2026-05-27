using System.Threading.Tasks;
using System.Xml.Linq;
using ArchivesSpaceWeb.Domains.Resources.Interfaces;
using ArchivesSpaceWeb.Domains.Shared.Interfaces;

namespace ArchivesSpaceWeb.Domains.Resources.Queries
{
    public class ExportEadQueryHandler : IQueryHandler<ExportEadQuery, XDocument?>
    {
        private readonly IResourceRepository _resourceRepository;
        private readonly IEadExportService _eadExportService;

        public ExportEadQueryHandler(IResourceRepository resourceRepository, IEadExportService eadExportService)
        {
            _resourceRepository = resourceRepository;
            _eadExportService = eadExportService;
        }

        public async Task<XDocument?> HandleAsync(ExportEadQuery query)
        {
            var resource = await _resourceRepository.GetResourceWithDetailsAsync(query.Id);
            if (resource == null) return null;

            var components = await _resourceRepository.GetComponentsTreeAsync(query.Id);

            return _eadExportService.GenerateEadXml(resource, components);
        }
    }
}
