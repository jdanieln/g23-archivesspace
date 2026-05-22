using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ArchivesSpaceWeb.Domains.Accessions.Entities;
using ArchivesSpaceWeb.Domains.Accessions.Interfaces;
using ArchivesSpaceWeb.Domains.Shared.Interfaces;
using ArchivesSpaceWeb.Domains.Admin.Entities;
using ArchivesSpaceWeb.Domains.Admin.Interfaces;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArchivesSpaceWeb.Domains.Accessions.Controllers
{
    [Authorize]
    public class AccessionController : Controller
    {
        private readonly IAccessionRepository _accessionRepository;
        private readonly IImportService _importService;
        private readonly IRepository<Event> _eventRepository;

        public AccessionController(
            IAccessionRepository accessionRepository,
            IImportService importService,
            IRepository<Event> eventRepository)
        {
            _accessionRepository = accessionRepository;
            _importService = importService;
            _eventRepository = eventRepository;
        }

        public async Task<IActionResult> Index()
        {
            var repClaim = User.FindFirst("RepositoryId")?.Value;
            var isSysAdmin = User.IsInRole("SystemAdmin");

            System.Collections.Generic.List<Accession> accessions;

            if (!isSysAdmin && int.TryParse(repClaim, out int repId) && repId > 0)
            {
                accessions = await _accessionRepository.GetByRepositoryAsync(repId);
            }
            else
            {
                accessions = await _accessionRepository.GetAllWithRepositoryAsync();
            }

            return View(accessions);
        }

        public async Task<IActionResult> Details(int id)
        {
            var accession = await _accessionRepository.GetAccessionWithDetailsAsync(id);

            if (accession == null) return NotFound();

            ViewBag.Agents = await _accessionRepository.GetAccessionAgentsAsync(id);

            return View(accession);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            if (User.IsInRole("ReadOnly")) return RedirectToAction("AccessDenied", "Account");
            ViewBag.Repositories = await _accessionRepository.GetAllRepositoriesAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Accession accession)
        {
            if (User.IsInRole("ReadOnly")) return RedirectToAction("AccessDenied", "Account");

            if (ModelState.IsValid)
            {
                await _accessionRepository.AddAsync(accession);
                await _accessionRepository.SaveChangesAsync();

                // Event logging (US 37)
                var creationEvent = new Event
                {
                    EventType = "Creation",
                    EventDate = DateTime.Now,
                    Description = $"Ingreso de Accession '{accession.Title}' ({accession.Identifier}) creado por {User.Identity?.Name}."
                };
                await _eventRepository.AddAsync(creationEvent);
                await _eventRepository.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            ViewBag.Repositories = await _accessionRepository.GetAllRepositoriesAsync();
            return View(accession);
        }

        // US 11: Import Accessions in CSV
        [HttpPost]
        public async Task<IActionResult> ImportCsv(IFormFile csvFile, int repositoryId)
        {
            if (User.IsInRole("ReadOnly") || User.IsInRole("BasicDataEntry"))
                return Json(new { success = false, message = "Acceso Denegado." });

            if (csvFile == null || csvFile.Length == 0)
            {
                return Json(new { success = false, message = "Selecciona un archivo CSV válido." });
            }

            try
            {
                using (var stream = csvFile.OpenReadStream())
                {
                    var result = await _importService.ImportAccessionsCsvAsync(stream, repositoryId);
                    if (result.Success)
                    {
                        return Json(new { success = true, successCount = result.SuccessCount, errorCount = result.ErrorCount, errorLog = result.LogDetails });
                    }
                    else
                    {
                        return Json(new { success = false, message = result.LogDetails });
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}
