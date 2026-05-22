using ArchivesSpaceWeb.Domains.Shared.Interfaces;
using ArchivesSpaceWeb.Domains.Resources.Entities;
using ArchivesSpaceWeb.Domains.Admin.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ArchivesSpaceWeb.Domains.Resources.Interfaces
{
    public interface IResourceRepository : IRepository<Resource>
    {
        Task<Resource?> GetResourceWithDetailsAsync(int id);
        Task<List<Resource>> GetAllWithRepositoryAsync();
        Task<List<Resource>> GetByRepositoryAsync(int repositoryId);
        Task<List<Repository>> GetAllRepositoriesAsync();
        Task<Repository?> GetRepositoryByIdAsync(int id);
        
        // Component hierarchy tree methods
        Task<List<ArchivalObject>> GetComponentsTreeAsync(int resourceId);
        Task<ArchivalObject?> GetArchivalObjectByIdAsync(int id);
        Task UpdateArchivalObjectAsync(ArchivalObject obj);
        Task AddArchivalObjectAsync(ArchivalObject obj);
        
        // Subrecord widgets
        Task<CollectionManagement?> GetCollectionManagementAsync(int resourceId);
        Task AddOrUpdateCollectionManagementAsync(CollectionManagement cm);
        Task<List<RightsStatement>> GetRightsStatementsAsync(int resourceId);
        Task UpdateRightsStatementsAsync(int resourceId, List<RightsStatement> rights);
        
        // Dynamic lists for Details
        Task<List<Subject>> GetResourceSubjectsAsync(int resourceId);
        Task<List<object>> GetResourceAgentsAsync(int resourceId); // Returns dynamic { Agent, Role } list
    }
}
