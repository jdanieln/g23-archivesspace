using ArchivesSpaceWeb.Domains.Shared.Interfaces;

namespace ArchivesSpaceWeb.Domains.Admin.Commands
{
    public class ResetPasswordCommand : ICommand<bool>
    {
        public int Id { get; }
        public string NewPassword { get; }

        public ResetPasswordCommand(int id, string newPassword)
        {
            Id = id;
            NewPassword = newPassword;
        }
    }
}
