using System;
using System.Threading.Tasks;
using ArchivesSpaceWeb.Domains.Admin.Entities;
using ArchivesSpaceWeb.Domains.Admin.Interfaces;
using ArchivesSpaceWeb.Domains.Shared.Interfaces;

namespace ArchivesSpaceWeb.Domains.Admin.Infrastructure
{
    public class AuditService : IAuditService
    {
        private readonly IRepository<Event> _eventRepository;

        public AuditService(IRepository<Event> eventRepository)
        {
            _eventRepository = eventRepository;
        }

        public async Task LogEventAsync(string eventType, string description, int? resourceId = null, int? agentId = null)
        {
            var newEvent = new Event
            {
                EventType = eventType,
                EventDate = DateTime.Now,
                Description = description,
                ResourceId = resourceId,
                AgentId = agentId
            };

            await _eventRepository.AddAsync(newEvent);
            await _eventRepository.SaveChangesAsync();
        }
    }
}
