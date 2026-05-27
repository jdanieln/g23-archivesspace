using System.Threading.Tasks;
using ArchivesSpaceWeb.Domains.Identity.Entities;
using ArchivesSpaceWeb.Domains.Identity.Interfaces;
using ArchivesSpaceWeb.Domains.Shared.Infrastructure;
using ArchivesSpaceWeb.Domains.Shared.Interfaces;

namespace ArchivesSpaceWeb.Domains.Admin.Commands
{
    public class CreateUserCommandHandler : ICommandHandler<CreateUserCommand, bool>
    {
        private readonly IUserRepository _userRepository;

        public CreateUserCommandHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<bool> HandleAsync(CreateUserCommand command)
        {
            var newUser = new User
            {
                Username = command.Username,
                PasswordHash = ApplicationDbContext.HashPassword(command.Password),
                Role = command.Role,
                RepositoryId = command.RepositoryId == 0 ? null : command.RepositoryId,
                AuthMode = command.AuthMode
            };

            await _userRepository.AddAsync(newUser);
            await _userRepository.SaveChangesAsync();
            return true;
        }
    }
}
