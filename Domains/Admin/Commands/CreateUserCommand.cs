using ArchivesSpaceWeb.Domains.Shared.Interfaces;

namespace ArchivesSpaceWeb.Domains.Admin.Commands
{
    public class CreateUserCommand : ICommand<bool>
    {
        public string Username { get; }
        public string Password { get; }
        public string Role { get; }
        public int? RepositoryId { get; }
        public string AuthMode { get; }

        public CreateUserCommand(string username, string password, string role, int? repositoryId, string authMode)
        {
            Username = username;
            Password = password;
            Role = role;
            RepositoryId = repositoryId;
            AuthMode = authMode;
        }
    }
}
