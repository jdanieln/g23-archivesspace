using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ArchivesSpaceWeb.Domains.Resources.Entities;
using ArchivesSpaceWeb.Domains.Resources.Interfaces;
using ArchivesSpaceWeb.Domains.Resources.Queries;
using ArchivesSpaceWeb.Domains.Resources.Commands;
using ArchivesSpaceWeb.Domains.Shared.Interfaces;
using ArchivesSpaceWeb.Domains.Admin.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ArchivesSpaceWeb.Domains.Resources.Controllers
{
    [Authorize]
    public class ResourceController : Controller
    {
        private readonly IQueryHandler<GetResourcesListQuery, List<Resource>> _listQueryHandler;
        private readonly IQueryHandler<GetResourceDetailsQuery, ResourceDetailsResult?> _detailsQueryHandler;
        private readonly IQueryHandler<GetResourceHierarchyQuery, List<ArchivalObject>> _hierarchyQueryHandler;
        private readonly IQueryHandler<ExportEadQuery, XDocument?> _exportQueryHandler;
        private readonly ICommandHandler<CreateResourceCommand, Resource> _createCommandHandler;
        private readonly ICommandHandler<EditResourceCommand, Resource> _editCommandHandler;
        private readonly ICommandHandler<UpdateResourceHierarchyCommand, bool> _updateHierarchyCommandHandler;
        private readonly IResourceRepository _resourceRepository;

        public ResourceController(
            IQueryHandler<GetResourcesListQuery, List<Resource>> listQueryHandler,
            IQueryHandler<GetResourceDetailsQuery, ResourceDetailsResult?> detailsQueryHandler,
            IQueryHandler<GetResourceHierarchyQuery, List<ArchivalObject>> hierarchyQueryHandler,
            IQueryHandler<ExportEadQuery, XDocument?> exportQueryHandler,
            ICommandHandler<CreateResourceCommand, Resource> createCommandHandler,
            ICommandHandler<EditResourceCommand, Resource> editCommandHandler,
            ICommandHandler<UpdateResourceHierarchyCommand, bool> updateHierarchyCommandHandler,
            IResourceRepository resourceRepository)
        {
            _listQueryHandler = listQueryHandler;
            _detailsQueryHandler = detailsQueryHandler;
            _hierarchyQueryHandler = hierarchyQueryHandler;
            _exportQueryHandler = exportQueryHandler;
            _createCommandHandler = createCommandHandler;
            _editCommandHandler = editCommandHandler;
            _updateHierarchyCommandHandler = updateHierarchyCommandHandler;
            _resourceRepository = resourceRepository;
        }

        public async Task<IActionResult> Index()
        {
            var query = new GetResourcesListQuery(User);
            var resources = await _listQueryHandler.HandleAsync(query);
            return View(resources);
        }

        public async Task<IActionResult> Details(int id)
        {
            var query = new GetResourceDetailsQuery(id);
            var result = await _detailsQueryHandler.HandleAsync(query);

            if (result == null) return NotFound();

            // Load subrecords & related tables (US 15 dates & extents display priority)
            ViewBag.Components = result.Components.Where(ao => ao.ParentId == null).ToList();
            ViewBag.Subjects = result.Subjects;
            ViewBag.Agents = result.Agents;
            ViewBag.CollectionManagement = result.CollectionManagement;
            ViewBag.RightsStatements = result.RightsStatements;

            return View(result.Resource);
        }

        [HttpGet]
        [Authorize(Roles = "SystemAdmin,RepositoryManager,BasicDataEntry")]
        public async Task<IActionResult> Create()
        {
            ViewBag.Repositories = await _resourceRepository.GetAllRepositoriesAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SystemAdmin,RepositoryManager,BasicDataEntry")]
        public async Task<IActionResult> Create(Resource resource)
        {
            if (ModelState.IsValid)
            {
                var command = new CreateResourceCommand(resource, User.Identity?.Name);
                await _createCommandHandler.HandleAsync(command);
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Repositories = await _resourceRepository.GetAllRepositoriesAsync();
            return View(resource);
        }

        [HttpGet]
        [Authorize(Roles = "SystemAdmin,RepositoryManager")]
        public async Task<IActionResult> Edit(int id)
        {
            var query = new GetResourceDetailsQuery(id);
            var result = await _detailsQueryHandler.HandleAsync(query);
            if (result == null) return NotFound();

            ViewBag.Repositories = await _resourceRepository.GetAllRepositoriesAsync();
            ViewBag.CollectionManagement = result.CollectionManagement ?? new CollectionManagement { ResourceId = id };
            ViewBag.RightsStatements = result.RightsStatements;

            return View(result.Resource);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SystemAdmin,RepositoryManager")]
        public async Task<IActionResult> Edit(int id, Resource resource, CollectionManagement? collMgmt, List<RightsStatement>? rights)
        {
            if (id != resource.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var command = new EditResourceCommand(id, resource, collMgmt, rights, User.Identity?.Name);
                    await _editCommandHandler.HandleAsync(command);
                }
                catch (DbUpdateConcurrencyException)
                {
                    // US 40: Overwrite prevention
                    ModelState.AddModelError(string.Empty, "Error: Otro usuario ha modificado este registro mientras lo editabas. Por favor recarga e intenta nuevamente.");
                    ViewBag.Repositories = await _resourceRepository.GetAllRepositoriesAsync();
                    ViewBag.CollectionManagement = collMgmt ?? new CollectionManagement { ResourceId = id };
                    ViewBag.RightsStatements = rights ?? new();
                    return View(resource);
                }

                return RedirectToAction(nameof(Details), new { id = resource.Id });
            }

            ViewBag.Repositories = await _resourceRepository.GetAllRepositoriesAsync();
            return View(resource);
        }

        // US 33 & 34: Keyboard & Drag and drop hierarchy editor
        [HttpGet]
        [Authorize(Roles = "SystemAdmin,RepositoryManager")]
        public async Task<IActionResult> EditHierarchy(int id)
        {
            var query = new GetResourceDetailsQuery(id);
            var result = await _detailsQueryHandler.HandleAsync(query);
            if (result == null) return NotFound();

            ViewBag.Components = result.Components;
            return View(result.Resource);
        }

        [HttpPost]
        [Authorize(Roles = "SystemAdmin,RepositoryManager")]
        public async Task<IActionResult> UpdateHierarchy([FromBody] List<HierarchyUpdateModel> updates)
        {
            if (updates == null || !updates.Any()) return BadRequest("No updates provided.");

            try
            {
                var command = new UpdateResourceHierarchyCommand(updates);
                await _updateHierarchyCommandHandler.HandleAsync(command);
                return Json(new { success = true, message = "Jerarquía guardada con éxito." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        // US 36: Export Description as EAD XML
        [HttpGet]
        public async Task<IActionResult> ExportEad(int id)
        {
            var query = new ExportEadQuery(id);
            var doc = await _exportQueryHandler.HandleAsync(query);

            if (doc == null) return NotFound();

            var stream = new MemoryStream();
            using (var writer = new StreamWriter(stream, Encoding.UTF8))
            {
                doc.Save(writer);
            }
            stream.Position = 0;

            // Query details for Identifier filename
            var detailsQuery = new GetResourceDetailsQuery(id);
            var details = await _detailsQueryHandler.HandleAsync(detailsQuery);
            string identifier = details?.Resource.Identifier ?? "EAD";

            string fileName = $"EAD-{identifier.Replace("/", "-")}.xml";
            return File(stream, "application/xml", fileName);
        }
    }
}
