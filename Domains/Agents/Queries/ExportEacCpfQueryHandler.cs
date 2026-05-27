using System.Threading.Tasks;
using System.Xml.Linq;
using ArchivesSpaceWeb.Domains.Agents.Interfaces;
using ArchivesSpaceWeb.Domains.Shared.Interfaces;

namespace ArchivesSpaceWeb.Domains.Agents.Queries
{
    public class ExportEacCpfQueryHandler : IQueryHandler<ExportEacCpfQuery, XDocument?>
    {
        private readonly IAgentRepository _agentRepository;
        private readonly IEacCpfExportService _eacCpfExportService;

        public ExportEacCpfQueryHandler(IAgentRepository agentRepository, IEacCpfExportService eacCpfExportService)
        {
            _agentRepository = agentRepository;
            _eacCpfExportService = eacCpfExportService;
        }

        public async Task<XDocument?> HandleAsync(ExportEacCpfQuery query)
        {
            var agent = await _agentRepository.GetByIdAsync(query.Id);
            if (agent == null) return null;

            return _eacCpfExportService.GenerateEacCpfXml(agent);
        }
    }
}
