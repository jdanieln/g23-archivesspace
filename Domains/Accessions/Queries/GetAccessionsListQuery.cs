using System.Collections.Generic;
using System.Security.Claims;
using ArchivesSpaceWeb.Domains.Accessions.Entities;
using ArchivesSpaceWeb.Domains.Shared.Interfaces;

namespace ArchivesSpaceWeb.Domains.Accessions.Queries
{
    public class GetAccessionsListQuery : IQuery<List<Accession>>
    {
        public ClaimsPrincipal User { get; }

        public GetAccessionsListQuery(ClaimsPrincipal user)
        {
            User = user;
        }
    }
}
