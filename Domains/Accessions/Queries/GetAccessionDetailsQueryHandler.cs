using System.Threading.Tasks;
using ArchivesSpaceWeb.Domains.Accessions.Interfaces;
using ArchivesSpaceWeb.Domains.Shared.Interfaces;

namespace ArchivesSpaceWeb.Domains.Accessions.Queries
{
    public class GetAccessionDetailsQueryHandler : IQueryHandler<GetAccessionDetailsQuery, AccessionDetailsResult?>
    {
        private readonly IAccessionRepository _accessionRepository;

        public GetAccessionDetailsQueryHandler(IAccessionRepository accessionRepository)
        {
            _accessionRepository = accessionRepository;
        }

        public async Task<AccessionDetailsResult?> HandleAsync(GetAccessionDetailsQuery query)
        {
            var accession = await _accessionRepository.GetAccessionWithDetailsAsync(query.Id);
            if (accession == null) return null;

            var agents = await _accessionRepository.GetAccessionAgentsAsync(query.Id);

            return new AccessionDetailsResult
            {
                Accession = accession,
                Agents = agents
            };
        }
    }
}
