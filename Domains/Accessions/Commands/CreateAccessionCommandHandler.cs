using System.Threading.Tasks;
using ArchivesSpaceWeb.Domains.Accessions.Entities;
using ArchivesSpaceWeb.Domains.Accessions.Interfaces;
using ArchivesSpaceWeb.Domains.Admin.Interfaces;
using ArchivesSpaceWeb.Domains.Shared.Interfaces;

namespace ArchivesSpaceWeb.Domains.Accessions.Commands
{
    public class CreateAccessionCommandHandler : ICommandHandler<CreateAccessionCommand, Accession>
    {
        private readonly IAccessionRepository _accessionRepository;
        private readonly IAuditService _auditService;

        public CreateAccessionCommandHandler(
            IAccessionRepository accessionRepository,
            IAuditService auditService)
        {
            _accessionRepository = accessionRepository;
            _auditService = auditService;
        }

        public async Task<Accession> HandleAsync(CreateAccessionCommand command)
        {
            await _accessionRepository.AddAsync(command.Accession);
            await _accessionRepository.SaveChangesAsync();

            // Event logging (US 37)
            await _auditService.LogEventAsync("Creation", $"Ingreso de Accession '{command.Accession.Title}' ({command.Accession.Identifier}) creado por {command.CreatedBy ?? "Sistema"}.");

            return command.Accession;
        }
    }
}
