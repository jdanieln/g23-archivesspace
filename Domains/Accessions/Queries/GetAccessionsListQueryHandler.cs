using System.Collections.Generic;
using System.Threading.Tasks;
using ArchivesSpaceWeb.Domains.Accessions.Entities;
using ArchivesSpaceWeb.Domains.Accessions.Interfaces;
using ArchivesSpaceWeb.Domains.Shared.Interfaces;

namespace ArchivesSpaceWeb.Domains.Accessions.Queries
{
    public class GetAccessionsListQueryHandler : IQueryHandler<GetAccessionsListQuery, List<Accession>>
    {
        private readonly IAccessionRepository _accessionRepository;

        public GetAccessionsListQueryHandler(IAccessionRepository accessionRepository)
        {
            _accessionRepository = accessionRepository;
        }

        public async Task<List<Accession>> HandleAsync(GetAccessionsListQuery query)
        {
            var repClaim = query.User.FindFirst("RepositoryId")?.Value;
            var isSysAdmin = query.User.IsInRole("SystemAdmin");

            if (!isSysAdmin && int.TryParse(repClaim, out int repId) && repId > 0)
            {
                return await _accessionRepository.GetByRepositoryAsync(repId);
            }
            
            return await _accessionRepository.GetAllWithRepositoryAsync();
        }
    }
}
