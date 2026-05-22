using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ArchivesSpaceWeb.Domains.Resources.Entities;
using ArchivesSpaceWeb.Domains.Resources.Interfaces;
using ArchivesSpaceWeb.Domains.Shared.Interfaces;
using ArchivesSpaceWeb.Domains.Admin.Entities;
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
        private readonly IResourceRepository _resourceRepository;
        private readonly IRepository<Event> _eventRepository;

        public ResourceController(
            IResourceRepository resourceRepository,
            IRepository<Event> eventRepository)
        {
            _resourceRepository = resourceRepository;
            _eventRepository = eventRepository;
        }

        public async Task<IActionResult> Index()
        {
            var repClaim = User.FindFirst("RepositoryId")?.Value;
            var isSysAdmin = User.IsInRole("SystemAdmin");

            List<Resource> resources;

            if (!isSysAdmin && int.TryParse(repClaim, out int repId) && repId > 0)
            {
                resources = await _resourceRepository.GetByRepositoryAsync(repId);
            }
            else
            {
                resources = await _resourceRepository.GetAllWithRepositoryAsync();
            }

            return View(resources);
        }

        public async Task<IActionResult> Details(int id)
        {
            var resource = await _resourceRepository.GetResourceWithDetailsAsync(id);

            if (resource == null) return NotFound();

            // Load subrecords & related tables (US 15 dates & extents display priority)
            var components = await _resourceRepository.GetComponentsTreeAsync(id);
            ViewBag.Components = components.Where(ao => ao.ParentId == null).ToList();

            ViewBag.Subjects = await _resourceRepository.GetResourceSubjectsAsync(id);
            ViewBag.Agents = await _resourceRepository.GetResourceAgentsAsync(id);

            ViewBag.CollectionManagement = await _resourceRepository.GetCollectionManagementAsync(id);
            ViewBag.RightsStatements = await _resourceRepository.GetRightsStatementsAsync(id);

            return View(resource);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            if (User.IsInRole("ReadOnly")) return RedirectToAction("AccessDenied", "Account");
            ViewBag.Repositories = await _resourceRepository.GetAllRepositoriesAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Resource resource)
        {
            if (User.IsInRole("ReadOnly")) return RedirectToAction("AccessDenied", "Account");

            if (ModelState.IsValid)
            {
                await _resourceRepository.AddAsync(resource);
                await _resourceRepository.SaveChangesAsync();

                // Log Event (US 37)
                var creationEvent = new Event
                {
                    EventType = "Creation",
                    EventDate = DateTime.Now,
                    Description = $"Recurso '{resource.Title}' ({resource.Identifier}) creado por {User.Identity?.Name}.",
                    ResourceId = resource.Id
                };
                await _eventRepository.AddAsync(creationEvent);
                await _eventRepository.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            ViewBag.Repositories = await _resourceRepository.GetAllRepositoriesAsync();
            return View(resource);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            if (User.IsInRole("ReadOnly") || User.IsInRole("BasicDataEntry")) 
                return RedirectToAction("AccessDenied", "Account");

            var resource = await _resourceRepository.GetByIdAsync(id);
            if (resource == null) return NotFound();

            ViewBag.Repositories = await _resourceRepository.GetAllRepositoriesAsync();
            
            // Subrecords load (US 32 and US 41)
            var cm = await _resourceRepository.GetCollectionManagementAsync(id);
            ViewBag.CollectionManagement = cm ?? new CollectionManagement { ResourceId = id };

            ViewBag.RightsStatements = await _resourceRepository.GetRightsStatementsAsync(id);

            return View(resource);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Resource resource, CollectionManagement? collMgmt, List<RightsStatement>? rights)
        {
            if (User.IsInRole("ReadOnly") || User.IsInRole("BasicDataEntry")) 
                return RedirectToAction("AccessDenied", "Account");

            if (id != resource.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    // US 40: Optimistic locking validation
                    await _resourceRepository.UpdateAsync(resource);
                    await _resourceRepository.SaveChangesAsync();

                    // US 32: Collection management save
                    if (collMgmt != null)
                    {
                        collMgmt.ResourceId = id;
                        await _resourceRepository.AddOrUpdateCollectionManagementAsync(collMgmt);
                    }

                    // US 41: Rights Management save
                    await _resourceRepository.UpdateRightsStatementsAsync(id, rights ?? new List<RightsStatement>());

                    // Log event (US 37)
                    var modificationEvent = new Event
                    {
                        EventType = "Modification",
                        EventDate = DateTime.Now,
                        Description = $"Recurso '{resource.Title}' modificado por {User.Identity?.Name}.",
                        ResourceId = resource.Id
                    };
                    await _eventRepository.AddAsync(modificationEvent);
                    await _resourceRepository.SaveChangesAsync();
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
        public async Task<IActionResult> EditHierarchy(int id)
        {
            var resource = await _resourceRepository.GetByIdAsync(id);
            if (resource == null) return NotFound();

            ViewBag.Components = await _resourceRepository.GetComponentsTreeAsync(id);
            return View(resource);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateHierarchy([FromBody] List<HierarchyUpdateModel> updates)
        {
            if (updates == null || !updates.Any()) return BadRequest("No updates provided.");

            try
            {
                foreach (var update in updates)
                {
                    var component = await _resourceRepository.GetArchivalObjectByIdAsync(update.Id);
                    if (component != null)
                    {
                        component.ParentId = update.ParentId == 0 ? null : update.ParentId;
                        component.Position = update.Position;
                        await _resourceRepository.UpdateArchivalObjectAsync(component);
                    }
                }
                await _resourceRepository.SaveChangesAsync();
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
            var resource = await _resourceRepository.GetResourceWithDetailsAsync(id);

            if (resource == null) return NotFound();

            var components = await _resourceRepository.GetComponentsTreeAsync(id);

            // Create compliance XML
            var doc = new XDocument(
                new XElement("ead",
                    new XAttribute("audience", "external"),
                    new XElement("eadheader",
                        new XElement("eadid", resource.Identifier),
                        new XElement("filedesc",
                            new XElement("titlestmt",
                                new XElement("titleproper", resource.Title)
                            ),
                            new XElement("publicationstmt",
                                new XElement("publisher", resource.Repository?.Name ?? "ArchivesSpaceWeb Repository"),
                                new XElement("p", $"Author: {resource.FindingAidAuthor ?? "Archivist Staff"}")
                            )
                        )
                    ),
                    new XElement("archdesc", new XAttribute("level", resource.LevelOfDescription.ToLower()),
                        new XElement("did",
                            new XElement("unittitle", resource.Title),
                            new XElement("unitid", resource.Identifier),
                            new XElement("unitdate", resource.Dates),
                            new XElement("physdesc",
                                new XElement("extent", resource.Extents)
                            ),
                            new XElement("langmaterial",
                                new XElement("language", resource.LanguageOfDescription ?? "Spanish")
                            )
                        ),
                        new XElement("dsc",
                            BuildXmlHierarchy(null, components)
                        )
                    )
                )
            );

            var stream = new MemoryStream();
            using (var writer = new StreamWriter(stream, Encoding.UTF8))
            {
                doc.Save(writer);
            }
            stream.Position = 0;

            string fileName = $"EAD-{resource.Identifier.Replace("/", "-")}.xml";
            return File(stream, "application/xml", fileName);
        }

        private List<XElement> BuildXmlHierarchy(int? parentId, List<ArchivalObject> list)
        {
            var elements = new List<XElement>();
            var levelChildren = list.Where(c => c.ParentId == parentId).OrderBy(c => c.Position);

            int counter = 1;
            foreach (var child in levelChildren)
            {
                var tag = parentId == null ? "c01" : "c"; // standard nested EAD component tag
                var childElem = new XElement(tag,
                    new XAttribute("level", child.LevelOfDescription.ToLower()),
                    new XAttribute("id", child.ComponentIdentifier ?? $"comp_{child.Id}"),
                    new XElement("did",
                        new XElement("unittitle", child.Title),
                        new XElement("unitid", child.ComponentIdentifier),
                        new XElement("unitdate", child.Dates),
                        new XElement("physdesc",
                            new XElement("extent", child.Extents)
                        )
                    )
                );

                var grandchildren = BuildXmlHierarchy(child.Id, list);
                if (grandchildren.Any())
                {
                    childElem.Add(grandchildren);
                }

                elements.Add(childElem);
                counter++;
            }

            return elements;
        }
    }

    public class HierarchyUpdateModel
    {
        public int Id { get; set; }
        public int? ParentId { get; set; }
        public int Position { get; set; }
    }
}
