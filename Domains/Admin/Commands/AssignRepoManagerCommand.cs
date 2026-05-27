using ArchivesSpaceWeb.Domains.Shared.Interfaces;

namespace ArchivesSpaceWeb.Domains.Admin.Commands
{
    public class AssignRepoManagerCommand : ICommand<bool>
    {
        public int Id { get; }
        public int RepositoryId { get; }

        public AssignRepoManagerCommand(int id, int repositoryId)
        {
            Id = id;
            RepositoryId = repositoryId;
        }
    }
}
