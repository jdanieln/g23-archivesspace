using System.Collections.Generic;
using ArchivesSpaceWeb.Domains.Accessions.Entities;
using ArchivesSpaceWeb.Domains.Agents.Entities;
using ArchivesSpaceWeb.Domains.Shared.Interfaces;

namespace ArchivesSpaceWeb.Domains.Accessions.Queries
{
    public class GetAccessionDetailsQuery : IQuery<AccessionDetailsResult?>
    {
        public int Id { get; }

        public GetAccessionDetailsQuery(int id)
        {
            Id = id;
        }
    }

    public class AccessionDetailsResult
    {
        public Accession Accession { get; set; } = null!;
        public List<object> Agents { get; set; } = new();
    }
}
