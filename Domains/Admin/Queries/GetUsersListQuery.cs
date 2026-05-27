using System.Collections.Generic;
using ArchivesSpaceWeb.Domains.Identity.Entities;
using ArchivesSpaceWeb.Domains.Admin.Entities;
using ArchivesSpaceWeb.Domains.Shared.Interfaces;

namespace ArchivesSpaceWeb.Domains.Admin.Queries
{
    public class GetUsersListQuery : IQuery<UsersListResult>
    {
    }

    public class UsersListResult
    {
        public List<User> Users { get; set; } = new();
        public List<Repository> Repositories { get; set; } = new();
    }
}
