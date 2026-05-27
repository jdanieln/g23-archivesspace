using System.Collections.Generic;
using System.Threading.Tasks;
using ArchivesSpaceWeb.Domains.Admin.Entities;
using ArchivesSpaceWeb.Domains.Admin.Interfaces;
using ArchivesSpaceWeb.Domains.Shared.Interfaces;

namespace ArchivesSpaceWeb.Domains.Admin.Queries
{
    public class GetEnumsListQueryHandler : IQueryHandler<GetEnumsListQuery, List<EnumList>>
    {
        private readonly IEnumService _enumService;

        public GetEnumsListQueryHandler(IEnumService enumService)
        {
            _enumService = enumService;
        }

        public async Task<List<EnumList>> HandleAsync(GetEnumsListQuery query)
        {
            return await _enumService.GetEnumListsAsync();
        }
    }
}
