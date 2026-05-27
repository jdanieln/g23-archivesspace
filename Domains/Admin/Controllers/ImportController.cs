using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ArchivesSpaceWeb.Domains.Admin.Entities;
using ArchivesSpaceWeb.Domains.Admin.Queries;
using ArchivesSpaceWeb.Domains.Admin.Commands;
using ArchivesSpaceWeb.Domains.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace ArchivesSpaceWeb.Domains.Admin.Controllers
{
    [Authorize]
    public class ImportController : Controller
    {
        private readonly IQueryHandler<GetImportLogsQuery, List<ImportLog>> _logsQueryHandler;
        private readonly IQueryHandler<GetUsersListQuery, UsersListResult> _usersListQueryHandler;
        private readonly ICommandHandler<ImportEadXmlCommand, ImportXmlResult> _importEadCommandHandler;
        private readonly ICommandHandler<ImportMarcXmlCommand, ImportXmlResult> _importMarcCommandHandler;
        private readonly ICommandHandler<ImportEacCpfXmlCommand, ImportXmlResult> _importEacCpfCommandHandler;

        public ImportController(
            IQueryHandler<GetImportLogsQuery, List<ImportLog>> logsQueryHandler,
            IQueryHandler<GetUsersListQuery, UsersListResult> usersListQueryHandler,
            ICommandHandler<ImportEadXmlCommand, ImportXmlResult> importEadCommandHandler,
            ICommandHandler<ImportMarcXmlCommand, ImportXmlResult> importMarcCommandHandler,
            ICommandHandler<ImportEacCpfXmlCommand, ImportXmlResult> importEacCpfCommandHandler)
        {
            _logsQueryHandler = logsQueryHandler;
            _usersListQueryHandler = usersListQueryHandler;
            _importEadCommandHandler = importEadCommandHandler;
            _importMarcCommandHandler = importMarcCommandHandler;
            _importEacCpfCommandHandler = importEacCpfCommandHandler;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var logs = await _logsQueryHandler.HandleAsync(new GetImportLogsQuery(20));
            var reposResult = await _usersListQueryHandler.HandleAsync(new GetUsersListQuery());
            ViewBag.Repositories = reposResult.Repositories;
            return View(logs);
        }

        [HttpPost]
        [Authorize(Roles = "SystemAdmin,RepositoryManager")]
        public async Task<IActionResult> ImportEad(IFormFile xmlFile, int repositoryId)
        {
            if (xmlFile == null || xmlFile.Length == 0)
                return Json(new { success = false, message = "Selecciona un archivo XML EAD válido." });

            try
            {
                using (var stream = xmlFile.OpenReadStream())
                {
                    var command = new ImportEadXmlCommand(stream, repositoryId);
                    var result = await _importEadCommandHandler.HandleAsync(command);
                    return Json(new { success = result.Success, message = result.Message, errorDetails = result.Success ? null : result.Message });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error durante la importación. Revisa la bitácora de errores.", errorDetails = ex.Message });
            }
        }

        [HttpPost]
        [Authorize(Roles = "SystemAdmin,RepositoryManager")]
        public async Task<IActionResult> ImportMarcXml(IFormFile xmlFile, int repositoryId, bool agentsAndSubjectsOnly)
        {
            if (xmlFile == null || xmlFile.Length == 0)
                return Json(new { success = false, message = "Selecciona un archivo MARCXML válido." });

            try
            {
                using (var stream = xmlFile.OpenReadStream())
                {
                    var command = new ImportMarcXmlCommand(stream, repositoryId, agentsAndSubjectsOnly);
                    var result = await _importMarcCommandHandler.HandleAsync(command);
                    return Json(new { success = result.Success, message = result.Message, errorDetails = result.Success ? null : result.Message });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error durante importación MARCXML. Revisa bitácora.", errorDetails = ex.Message });
            }
        }

        [HttpPost]
        [Authorize(Roles = "SystemAdmin,RepositoryManager")]
        public async Task<IActionResult> ImportEacCpf(IFormFile xmlFile)
        {
            if (xmlFile == null || xmlFile.Length == 0)
                return Json(new { success = false, message = "Selecciona un archivo EAC-CPF válido." });

            try
            {
                using (var stream = xmlFile.OpenReadStream())
                {
                    var command = new ImportEacCpfXmlCommand(stream);
                    var result = await _importEacCpfCommandHandler.HandleAsync(command);
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
