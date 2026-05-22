using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ArchivesSpaceWeb.Domains.Resources.Entities;
using ArchivesSpaceWeb.Domains.Resources.Interfaces;
using System.Linq;
using System.Threading.Tasks;

namespace ArchivesSpaceWeb.Domains.Resources.Controllers
{
    [AllowAnonymous] // Finding Aid is public for researchers (US 1)
    public class FindingAidController : Controller
    {
        private readonly IResourceRepository _resourceRepository;

        public FindingAidController(IResourceRepository resourceRepository)
        {
            _resourceRepository = resourceRepository;
        }

        public async Task<IActionResult> Index(string search)
        {
            var resources = await _resourceRepository.GetAllWithRepositoryAsync();
            
            if (!string.IsNullOrEmpty(search))
            {
                search = search.ToLower();
                resources = resources.Where(r => r.Title.ToLower().Contains(search) || r.Identifier.ToLower().Contains(search)).ToList();
            }

            ViewBag.Search = search;
            return View(resources);
        }

        public async Task<IActionResult> View(int id)
        {
            var resource = await _resourceRepository.GetResourceWithDetailsAsync(id);

            if (resource == null) return NotFound();

            // Load components hierarchy recursively in order
            ViewBag.Components = await _resourceRepository.GetComponentsTreeAsync(id);

            // Load related subjects
            ViewBag.Subjects = await _resourceRepository.GetResourceSubjectsAsync(id);

            // Load related agents
            ViewBag.Agents = await _resourceRepository.GetResourceAgentsAsync(id);

            // Load collection management subrecord details
            ViewBag.CollectionManagement = await _resourceRepository.GetCollectionManagementAsync(id);

            // Load linked rights statements
            ViewBag.RightsStatements = await _resourceRepository.GetRightsStatementsAsync(id);

            return View(resource);
        }
    }
}
