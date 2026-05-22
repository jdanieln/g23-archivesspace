using System.Collections.Generic;
using System.Threading.Tasks;
using ArchivesSpaceWeb.Domains.Accessions.Entities;
using ArchivesSpaceWeb.Domains.Shared.Interfaces;
using ArchivesSpaceWeb.Domains.Admin.Entities;

namespace ArchivesSpaceWeb.Domains.Accessions.Interfaces
{
    public interface IAccessionRepository : IRepository<Accession>
    {
        Task<Accession?> GetAccessionWithDetailsAsync(int id);
        Task<List<Accession>> GetAllWithRepositoryAsync();
        Task<List<Accession>> GetByRepositoryAsync(int repositoryId);
        Task<List<object>> GetAccessionAgentsAsync(int accessionId);
        Task<List<Repository>> GetAllRepositoriesAsync();
    }
}
