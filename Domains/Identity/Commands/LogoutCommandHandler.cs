using System.Threading.Tasks;
using ArchivesSpaceWeb.Domains.Admin.Interfaces;
using ArchivesSpaceWeb.Domains.Shared.Interfaces;

namespace ArchivesSpaceWeb.Domains.Identity.Commands
{
    public class LogoutCommandHandler : ICommandHandler<LogoutCommand, bool>
    {
        private readonly IAuditService _auditService;

        public LogoutCommandHandler(IAuditService auditService)
        {
            _auditService = auditService;
        }

        public async Task<bool> HandleAsync(LogoutCommand command)
        {
            if (!string.IsNullOrEmpty(command.Username))
            {
                await _auditService.LogEventAsync("Logout", $"Usuario '{command.Username}' cerró sesión.");
            }
            return true;
        }
    }
}
