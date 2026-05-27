using ArchivesSpaceWeb.Domains.Shared.Interfaces;

namespace ArchivesSpaceWeb.Domains.Identity.Commands
{
    public class LogoutCommand : ICommand<bool>
    {
        public string? Username { get; }

        public LogoutCommand(string? username)
        {
            Username = username;
        }
    }
}
