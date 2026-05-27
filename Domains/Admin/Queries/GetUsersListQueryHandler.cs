using System.Threading.Tasks;
using ArchivesSpaceWeb.Domains.Identity.Interfaces;
using ArchivesSpaceWeb.Domains.Admin.Entities;
using ArchivesSpaceWeb.Domains.Shared.Interfaces;

namespace ArchivesSpaceWeb.Domains.Admin.Queries
{
    public class GetUsersListQueryHandler : IQueryHandler<GetUsersListQuery, UsersListResult>
    {
        private readonly IUserRepository _userRepository;
        private readonly IRepository<Repository> _repositoryRepo;

        public GetUsersListQueryHandler(IUserRepository userRepository, IRepository<Repository> repositoryRepo)
        {
            _userRepository = userRepository;
            _repositoryRepo = repositoryRepo;
        }

        public async Task<UsersListResult> HandleAsync(GetUsersListQuery query)
        {
            var users = await _userRepository.GetUsersWithRepositoryAsync();
            var repositories = await _repositoryRepo.GetAllAsync();

            return new UsersListResult
            {
                Users = users,
                Repositories = repositories
            };
        }
    }
}
