using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ArchivesSpaceWeb.Domains.Shared.Infrastructure;
using ArchivesSpaceWeb.Domains.Resources.Interfaces;
using ArchivesSpaceWeb.Domains.Resources.Entities;
using ArchivesSpaceWeb.Domains.Admin.Entities;

namespace ArchivesSpaceWeb.Domains.Resources.Infrastructure
{
    public class EFResourceRepository : EFGenericRepository<Resource>, IResourceRepository
    {
        public EFResourceRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<Resource?> GetResourceWithDetailsAsync(int id)
        {
            return await _context.Resources
                .Include(r => r.Repository)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<List<Resource>> GetAllWithRepositoryAsync()
        {
            return await _context.Resources
                .Include(r => r.Repository)
                .ToListAsync();
        }

        public async Task<List<Resource>> GetByRepositoryAsync(int repositoryId)
        {
            return await _context.Resources
                .Include(r => r.Repository)
                .Where(r => r.RepositoryId == repositoryId)
                .ToListAsync();
        }

        public async Task<List<Repository>> GetAllRepositoriesAsync()
        {
            return await _context.Repositories.ToListAsync();
        }

        public async Task<Repository?> GetRepositoryByIdAsync(int id)
        {
            return await _context.Repositories.FindAsync(id);
        }

        // Component hierarchy tree methods
        public async Task<List<ArchivalObject>> GetComponentsTreeAsync(int resourceId)
        {
            return await _context.ArchivalObjects
                .Where(ao => ao.ResourceId == resourceId)
                .OrderBy(ao => ao.ParentId)
                .ThenBy(ao => ao.Position)
                .ToListAsync();
        }

        public async Task<ArchivalObject?> GetArchivalObjectByIdAsync(int id)
        {
            return await _context.ArchivalObjects.FindAsync(id);
        }

        public async Task UpdateArchivalObjectAsync(ArchivalObject obj)
        {
            _context.ArchivalObjects.Update(obj);
            await Task.CompletedTask;
        }

        public async Task AddArchivalObjectAsync(ArchivalObject obj)
        {
            await _context.ArchivalObjects.AddAsync(obj);
        }

        // Subrecord widgets
        public async Task<CollectionManagement?> GetCollectionManagementAsync(int resourceId)
        {
            return await _context.CollectionManagements
                .FirstOrDefaultAsync(cm => cm.ResourceId == resourceId);
        }

        public async Task AddOrUpdateCollectionManagementAsync(CollectionManagement cm)
        {
            var dbCm = await _context.CollectionManagements.FirstOrDefaultAsync(c => c.ResourceId == cm.ResourceId);
            if (dbCm == null)
            {
                await _context.CollectionManagements.AddAsync(cm);
            }
            else
            {
                dbCm.ProcessingStatus = cm.ProcessingStatus;
                dbCm.ProcessingPriority = cm.ProcessingPriority;
                dbCm.ProcessingNotes = cm.ProcessingNotes;
                _context.CollectionManagements.Update(dbCm);
            }
        }

        public async Task<List<RightsStatement>> GetRightsStatementsAsync(int resourceId)
        {
            return await _context.RightsStatements
                .Where(rs => rs.ResourceId == resourceId)
                .ToListAsync();
        }

        public async Task UpdateRightsStatementsAsync(int resourceId, List<RightsStatement> rights)
        {
            var existingRights = await _context.RightsStatements.Where(r => r.ResourceId == resourceId).ToListAsync();
            _context.RightsStatements.RemoveRange(existingRights);
            if (rights != null)
            {
                foreach (var right in rights.Where(r => !string.IsNullOrEmpty(r.RightsType)))
                {
                    right.ResourceId = resourceId;
                    right.Id = 0; // Fresh insert
                    await _context.RightsStatements.AddAsync(right);
                }
            }
        }

        // Dynamic lists for Details
        public async Task<List<Subject>> GetResourceSubjectsAsync(int resourceId)
        {
            return await _context.ResourceSubjects
                .Include(rs => rs.Subject)
                .Where(rs => rs.ResourceId == resourceId)
                .Select(rs => rs.Subject!)
                .ToListAsync();
        }

        public async Task<List<object>> GetResourceAgentsAsync(int resourceId)
        {
            var list = await _context.ResourceAgents
                .Include(ra => ra.Agent)
                .Where(ra => ra.ResourceId == resourceId)
                .ToListAsync();
            
            return list.Select(ra => (object)new { ra.Agent, ra.Role }).ToList();
        }
    }
}
