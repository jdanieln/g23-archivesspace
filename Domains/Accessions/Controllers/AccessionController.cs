using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ArchivesSpaceWeb.Domains.Accessions.Entities;
using ArchivesSpaceWeb.Domains.Accessions.Interfaces;
using ArchivesSpaceWeb.Domains.Accessions.Queries;
using ArchivesSpaceWeb.Domains.Accessions.Commands;
using ArchivesSpaceWeb.Domains.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace ArchivesSpaceWeb.Domains.Accessions.Controllers
{
    [Authorize]
    public class AccessionController : Controller
    {
        private readonly IQueryHandler<GetAccessionsListQuery, List<Accession>> _listQueryHandler;
        private readonly IQueryHandler<GetAccessionDetailsQuery, AccessionDetailsResult?> _detailsQueryHandler;
        private readonly ICommandHandler<CreateAccessionCommand, Accession> _createCommandHandler;
        private readonly ICommandHandler<ImportAccessionsCsvCommand, ImportAccessionsCsvResult> _importCommandHandler;
        private readonly IAccessionRepository _accessionRepository;

        public AccessionController(
            IQueryHandler<GetAccessionsListQuery, List<Accession>> listQueryHandler,
            IQueryHandler<GetAccessionDetailsQuery, AccessionDetailsResult?> detailsQueryHandler,
            ICommandHandler<CreateAccessionCommand, Accession> createCommandHandler,
            ICommandHandler<ImportAccessionsCsvCommand, ImportAccessionsCsvResult> importCommandHandler,
            IAccessionRepository accessionRepository)
        {
            _listQueryHandler = listQueryHandler;
            _detailsQueryHandler = detailsQueryHandler;
            _createCommandHandler = createCommandHandler;
            _importCommandHandler = importCommandHandler;
            _accessionRepository = accessionRepository;
        }

        public async Task<IActionResult> Index()
        {
            var query = new GetAccessionsListQuery(User);
            var accessions = await _listQueryHandler.HandleAsync(query);
            return View(accessions);
        }

        public async Task<IActionResult> Details(int id)
        {
            var query = new GetAccessionDetailsQuery(id);
            var result = await _detailsQueryHandler.HandleAsync(query);

            if (result == null) return NotFound();

            ViewBag.Agents = result.Agents;
            return View(result.Accession);
        }

        [HttpGet]
        [Authorize(Roles = "SystemAdmin,RepositoryManager,BasicDataEntry")]
        public async Task<IActionResult> Create()
        {
            ViewBag.Repositories = await _accessionRepository.GetAllRepositoriesAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SystemAdmin,RepositoryManager,BasicDataEntry")]
        public async Task<IActionResult> Create(Accession accession)
        {
            if (ModelState.IsValid)
            {
                var command = new CreateAccessionCommand(accession, User.Identity?.Name);
                await _createCommandHandler.HandleAsync(command);
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Repositories = await _accessionRepository.GetAllRepositoriesAsync();
            return View(accession);
        }

        // US 11: Import Accessions in CSV
        [HttpPost]
        [Authorize(Roles = "SystemAdmin,RepositoryManager")]
        public async Task<IActionResult> ImportCsv(IFormFile csvFile, int repositoryId)
        {
            if (csvFile == null || csvFile.Length == 0)
            {
                return Json(new { success = false, message = "Selecciona un archivo CSV válido." });
            }

            try
            {
                using (var stream = csvFile.OpenReadStream())
                {
                    var command = new ImportAccessionsCsvCommand(stream, repositoryId);
                    var result = await _importCommandHandler.HandleAsync(command);
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
