using ArchivesSpaceWeb.Domains.Accessions.Entities;
using ArchivesSpaceWeb.Domains.Shared.Interfaces;

namespace ArchivesSpaceWeb.Domains.Accessions.Commands
{
    public class CreateAccessionCommand : ICommand<Accession>
    {
        public Accession Accession { get; }
        public string? CreatedBy { get; }

        public CreateAccessionCommand(Accession accession, string? createdBy)
        {
            Accession = accession;
            CreatedBy = createdBy;
        }
    }
}
