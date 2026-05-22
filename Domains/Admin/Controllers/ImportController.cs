using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ArchivesSpaceWeb.Domains.Admin.Interfaces;
using System;
using System.IO;
using System.Threading.Tasks;

namespace ArchivesSpaceWeb.Domains.Admin.Controllers
{
    [Authorize]
    public class ImportController : Controller
    {
        private readonly IImportService _importService;

        public ImportController(IImportService importService)
        {
            _importService = importService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var logs = await _importService.GetRecentImportLogsAsync(20);
            ViewBag.Repositories = await _importService.GetAllRepositoriesAsync();
            return View(logs);
        }

        // US 4, 29, 42, 43: EAD XML Importer
        [HttpPost]
        public async Task<IActionResult> ImportEad(IFormFile xmlFile, int repositoryId)
        {
            if (User.IsInRole("ReadOnly") || User.IsInRole("BasicDataEntry"))
                return Json(new { success = false, message = "Acceso Denegado." });

            if (xmlFile == null || xmlFile.Length == 0)
                return Json(new { success = false, message = "Selecciona un archivo XML EAD válido." });

            try
            {
                using (var stream = xmlFile.OpenReadStream())
                {
                    var result = await _importService.ImportEadAsync(stream, repositoryId);
                    return Json(new { success = result.Success, message = result.Message, errorDetails = result.Success ? null : result.Message });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error durante la importación. Revisa la bitácora de errores.", errorDetails = ex.Message });
            }
        }

        // US 12 & 20: MARCXML Importer
        [HttpPost]
        public async Task<IActionResult> ImportMarcXml(IFormFile xmlFile, int repositoryId, bool agentsAndSubjectsOnly)
        {
            if (User.IsInRole("ReadOnly") || User.IsInRole("BasicDataEntry"))
                return Json(new { success = false, message = "Acceso Denegado." });

            if (xmlFile == null || xmlFile.Length == 0)
                return Json(new { success = false, message = "Selecciona un archivo MARCXML válido." });

            try
            {
                using (var stream = xmlFile.OpenReadStream())
                {
                    var result = await _importService.ImportMarcXmlAsync(stream, repositoryId, agentsAndSubjectsOnly);
                    return Json(new { success = result.Success, message = result.Message, errorDetails = result.Success ? null : result.Message });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error durante importación MARCXML. Revisa bitácora.", errorDetails = ex.Message });
            }
        }

        // US 16: EAC-CPF XML Agent Importer
        [HttpPost]
        public async Task<IActionResult> ImportEacCpf(IFormFile xmlFile)
        {
            if (User.IsInRole("ReadOnly") || User.IsInRole("BasicDataEntry"))
                return Json(new { success = false, message = "Acceso Denegado." });

            if (xmlFile == null || xmlFile.Length == 0)
                return Json(new { success = false, message = "Selecciona un archivo EAC-CPF válido." });

            try
            {
                using (var stream = xmlFile.OpenReadStream())
                {
                    var result = await _importService.ImportEacCpfAsync(stream);
                    return Json(new { success = result.Success, message = result.Message, errorDetails = result.Success ? null : result.Message });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error durante importación EAC-CPF.", errorDetails = ex.Message });
            }
        }
    }
}
