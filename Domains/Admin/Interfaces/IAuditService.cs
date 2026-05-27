using System.Threading.Tasks;

namespace ArchivesSpaceWeb.Domains.Admin.Interfaces
{
    public interface IAuditService
    {
        Task LogEventAsync(string eventType, string description, int? resourceId = null, int? agentId = null);
    }
}
