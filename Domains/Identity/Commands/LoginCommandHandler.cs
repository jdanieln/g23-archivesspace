using System.Threading.Tasks;
using ArchivesSpaceWeb.Domains.Identity.Entities;
using ArchivesSpaceWeb.Domains.Identity.Interfaces;
using ArchivesSpaceWeb.Domains.Admin.Interfaces;
using ArchivesSpaceWeb.Domains.Shared.Interfaces;

namespace ArchivesSpaceWeb.Domains.Identity.Commands
{
    public class LoginCommandHandler : ICommandHandler<LoginCommand, User?>
    {
        private readonly IAuthService _authService;
        private readonly IAuditService _auditService;

        public LoginCommandHandler(IAuthService authService, IAuditService auditService)
        {
            _authService = authService;
            _auditService = auditService;
        }

        public async Task<User?> HandleAsync(LoginCommand command)
        {
            var user = await _authService.ValidateUserAsync(command.Username, command.Password, command.AuthMode);
            if (user != null)
            {
                await _auditService.LogEventAsync("Login", $"Usuario '{user.Username}' inició sesión usando método '{user.AuthMode}' con rol '{user.Role}'.");
            }
            return user;
        }
    }
}
