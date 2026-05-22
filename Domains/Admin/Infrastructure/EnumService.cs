using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ArchivesSpaceWeb.Domains.Admin.Entities;
using ArchivesSpaceWeb.Domains.Admin.Interfaces;
using ArchivesSpaceWeb.Domains.Shared.Infrastructure;
using ArchivesSpaceWeb.Domains.Resources.Entities;
using ArchivesSpaceWeb.Domains.Accessions.Entities;

namespace ArchivesSpaceWeb.Domains.Admin.Infrastructure
{
    public class EnumService : IEnumService
    {
        private readonly ApplicationDbContext _context;

        public EnumService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<EnumList>> GetEnumListsAsync()
        {
            return await _context.EnumLists
                .Include(e => e.EnumValues)
                .ToListAsync();
        }

        public async Task AddEnumValueAsync(int enumListId, string value, string label)
        {
            if (string.IsNullOrEmpty(value) || string.IsNullOrEmpty(label))
            {
                throw new ArgumentException("Value and Label are required.");
            }

            var enumVal = new EnumValue
            {
                EnumListId = enumListId,
                Value = value,
                Label = label
            };

            _context.EnumValues.Add(enumVal);
            await _context.SaveChangesAsync();
        }

        public async Task<int> BulkUpdateEnumValuesAsync(int enumListId, string oldValue, string newValue)
        {
            if (string.IsNullOrEmpty(oldValue) || string.IsNullOrEmpty(newValue))
            {
                throw new ArgumentException("Old value and New value are required.");
            }

            var list = await _context.EnumLists.FindAsync(enumListId);
            if (list == null)
            {
                throw new KeyNotFoundException("Enum list not found.");
            }

            int affectedRows = 0;

            if (list.Name == "level_of_description")
            {
                // Update Resources
                var resources = await _context.Resources.Where(r => r.LevelOfDescription == oldValue).ToListAsync();
                foreach (var r in resources)
                {
                    r.LevelOfDescription = newValue;
                    _context.Resources.Update(r);
                    affectedRows++;
                }

                // Update Components
                var comps = await _context.ArchivalObjects.Where(c => c.LevelOfDescription == oldValue).ToListAsync();
                foreach (var c in comps)
                {
                    c.LevelOfDescription = newValue;
                    _context.ArchivalObjects.Update(c);
                    affectedRows++;
                }
            }
            else if (list.Name == "agent_role")
            {
                // Update Resource Agents role
                var resAgents = await _context.ResourceAgents.Where(ra => ra.Role == oldValue).ToListAsync();
                foreach (var ra in resAgents)
                {
                    ra.Role = newValue;
                    _context.ResourceAgents.Update(ra);
                    affectedRows++;
                }

                // Update Accession Agents role
                var accAgents = await _context.AccessionAgents.Where(aa => aa.Role == oldValue).ToListAsync();
                foreach (var aa in accAgents)
                {
                    aa.Role = newValue;
                    _context.AccessionAgents.Update(aa);
                    affectedRows++;
                }
            }
            else if (list.Name == "processing_status")
            {
                var colManagements = await _context.CollectionManagements.Where(cm => cm.ProcessingStatus == oldValue).ToListAsync();
                foreach (var cm in colManagements)
                {
                    cm.ProcessingStatus = newValue;
                    _context.CollectionManagements.Update(cm);
                    affectedRows++;
                }
            }

            await _context.SaveChangesAsync();
            return affectedRows;
        }
    }
}
