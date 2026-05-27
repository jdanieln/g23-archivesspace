using ArchivesSpaceWeb.Domains.Identity.Entities;
using ArchivesSpaceWeb.Domains.Shared.Interfaces;

namespace ArchivesSpaceWeb.Domains.Identity.Commands
{
    public class LoginCommand : ICommand<User?>
    {
        public string Username { get; }
        public string Password { get; }
        public string AuthMode { get; }

        public LoginCommand(string username, string password, string authMode)
        {
            Username = username;
            Password = password;
            AuthMode = authMode;
        }
    }
}
