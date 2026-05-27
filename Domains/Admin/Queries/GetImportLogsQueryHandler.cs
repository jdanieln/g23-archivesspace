using System.Collections.Generic;
using System.Threading.Tasks;
using ArchivesSpaceWeb.Domains.Admin.Entities;
using ArchivesSpaceWeb.Domains.Admin.Interfaces;
using ArchivesSpaceWeb.Domains.Shared.Interfaces;

namespace ArchivesSpaceWeb.Domains.Admin.Queries
{
    public class GetImportLogsQueryHandler : IQueryHandler<GetImportLogsQuery, List<ImportLog>>
    {
        private readonly IImportService _importService;

        public GetImportLogsQueryHandler(IImportService importService)
        {
            _importService = importService;
        }

        public async Task<List<ImportLog>> HandleAsync(GetImportLogsQuery query)
        {
            return await _importService.GetRecentImportLogsAsync(query.Count);
        }
    }
}
