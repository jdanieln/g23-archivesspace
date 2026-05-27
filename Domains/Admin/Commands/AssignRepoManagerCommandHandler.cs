using System.Threading.Tasks;
using ArchivesSpaceWeb.Domains.Identity.Interfaces;
using ArchivesSpaceWeb.Domains.Shared.Interfaces;

namespace ArchivesSpaceWeb.Domains.Admin.Commands
{
    public class AssignRepoManagerCommandHandler : ICommandHandler<AssignRepoManagerCommand, bool>
    {
        private readonly IUserRepository _userRepository;

        public AssignRepoManagerCommandHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<bool> HandleAsync(AssignRepoManagerCommand command)
        {
            var user = await _userRepository.GetByIdAsync(command.Id);
            if (user == null) return false;

            user.Role = "RepositoryManager";
            user.RepositoryId = command.RepositoryId;
            await _userRepository.UpdateAsync(user);
            await _userRepository.SaveChangesAsync();
            return true;
        }
    }
}
