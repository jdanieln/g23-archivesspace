using System.Collections.Generic;
using System.Security.Claims;
using ArchivesSpaceWeb.Domains.Resources.Entities;
using ArchivesSpaceWeb.Domains.Shared.Interfaces;

namespace ArchivesSpaceWeb.Domains.Resources.Queries
{
    public class GetResourcesListQuery : IQuery<List<Resource>>
    {
        public ClaimsPrincipal User { get; }

        public GetResourcesListQuery(ClaimsPrincipal user)
        {
            User = user;
        }
    }
}
