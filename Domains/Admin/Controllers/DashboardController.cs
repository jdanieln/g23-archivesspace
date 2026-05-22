using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ArchivesSpaceWeb.Domains.Admin.Entities;
using ArchivesSpaceWeb.Domains.Resources.Entities;
using ArchivesSpaceWeb.Domains.Resources.Interfaces;
using ArchivesSpaceWeb.Domains.Accessions.Entities;
using ArchivesSpaceWeb.Domains.Accessions.Interfaces;
using ArchivesSpaceWeb.Domains.Agents.Entities;
using ArchivesSpaceWeb.Domains.Shared.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace ArchivesSpaceWeb.Domains.Admin.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly IRepository<Repository> _repositoryRepo;
        private readonly IResourceRepository _resourceRepository;
        private readonly IAccessionRepository _accessionRepository;
        private readonly IRepository<Agent> _agentRepo;
        private readonly IRepository<Subject> _subjectRepo;
        private readonly IRepository<DigitalObject> _digitalObjectRepo;
        private readonly IRepository<Event> _eventRepo;

        public DashboardController(
            IRepository<Repository> repositoryRepo,
            IResourceRepository resourceRepository,
            IAccessionRepository accessionRepository,
            IRepository<Agent> agentRepo,
            IRepository<Subject> subjectRepo,
            IRepository<DigitalObject> digitalObjectRepo,
            IRepository<Event> eventRepo)
        {
            _repositoryRepo = repositoryRepo;
            _resourceRepository = resourceRepository;
            _accessionRepository = accessionRepository;
            _agentRepo = agentRepo;
            _subjectRepo = subjectRepo;
            _digitalObjectRepo = digitalObjectRepo;
            _eventRepo = eventRepo;
        }

        public async Task<IActionResult> Index()
        {
            // Collect system-wide stats
            var repositories = await _repositoryRepo.GetAllAsync();
            var resources = await _resourceRepository.GetAllAsync();
            var accessions = await _accessionRepository.GetAllAsync();
            var agents = await _agentRepo.GetAllAsync();
            var subjects = await _subjectRepo.GetAllAsync();
            var digitalObjects = await _digitalObjectRepo.GetAllAsync();

            ViewBag.RepositoryCount = repositories.Count;
            ViewBag.ResourceCount = resources.Count;
            ViewBag.AccessionCount = accessions.Count;
            ViewBag.AgentCount = agents.Count;
            ViewBag.SubjectCount = subjects.Count;
            ViewBag.DigitalObjectCount = digitalObjects.Count;

            // Fetch recent system events
            var allEvents = await _eventRepo.GetAllAsync();
            var recentEvents = allEvents
                .OrderByDescending(e => e.EventDate)
                .Take(6)
                .ToList();

            // Pass active repository details
            var repClaim = User.FindFirst("RepositoryId")?.Value;
            if (int.TryParse(repClaim, out int repId) && repId > 0)
            {
                var activeRepo = await _repositoryRepo.GetByIdAsync(repId);
                ViewBag.ActiveRepositoryName = activeRepo?.Name ?? "Ninguno Seleccionado";
            }
            else
            {
                ViewBag.ActiveRepositoryName = "Ninguno Seleccionado (Administrador del Sistema)";
            }

            return View(recentEvents);
        }

        // US 31: Search within a repository for resource, accession and digital object records
        [HttpGet]
        public async Task<IActionResult> Search(int? repositoryId, string query)
        {
            // If repositoryId is not provided, try to pull it from the user's claims
            if (!repositoryId.HasValue)
            {
                var repClaim = User.FindFirst("RepositoryId")?.Value;
                if (int.TryParse(repClaim, out int parsedRepId) && parsedRepId > 0)
                {
                    repositoryId = parsedRepId;
                }
            }

            ViewBag.Query = query;
            ViewBag.SelectedRepositoryId = repositoryId;
            ViewBag.Repositories = await _repositoryRepo.GetAllAsync();

            if (string.IsNullOrEmpty(query))
            {
                return View(new SearchResultViewModel());
            }

            query = query.ToLower().Trim();

            // Fetch all and filter in-memory to keep repositories highly abstract,
            // or fetch filtered collections.
            var allResources = await _resourceRepository.GetAllWithRepositoryAsync();
            var allAccessions = await _accessionRepository.GetAllWithRepositoryAsync();
            
            // For digital objects, we get all with repository. Since they belong to repository, let's query them.
            // Note: Since digital object is simple, let's load all and filter or query them.
            var allDigitalObjects = await _digitalObjectRepo.GetAllAsync();
            
            // To include repository on digital objects, let's load repositories too and map, or since they are virtual:
            // EF Core will automatically map the repository if tracked. Let's do that!
            
            var resourceFilter = allResources.AsQueryable();
            var accessionFilter = allAccessions.AsQueryable();
            var digitalObjectFilter = allDigitalObjects.AsQueryable();

            if (repositoryId.HasValue && repositoryId > 0)
            {
                resourceFilter = resourceFilter.Where(r => r.RepositoryId == repositoryId.Value);
                accessionFilter = accessionFilter.Where(a => a.RepositoryId == repositoryId.Value);
                digitalObjectFilter = digitalObjectFilter.Where(d => d.RepositoryId == repositoryId.Value);
            }

            var resourcesResult = resourceFilter
                .Where(r => r.Title.ToLower().Contains(query) || r.Identifier.ToLower().Contains(query) || (r.Dates != null && r.Dates.ToLower().Contains(query)))
                .ToList();

            var accessionsResult = accessionFilter
                .Where(a => a.Title.ToLower().Contains(query) || a.Identifier.ToLower().Contains(query) || (a.Dates != null && a.Dates.ToLower().Contains(query)))
                .ToList();

            var digitalObjectsResult = digitalObjectFilter
                .Where(d => d.Title.ToLower().Contains(query) || d.Identifier.ToLower().Contains(query) || d.FileUri.ToLower().Contains(query))
                .ToList();

            var results = new SearchResultViewModel
            {
                Resources = resourcesResult,
                Accessions = accessionsResult,
                DigitalObjects = digitalObjectsResult
            };

            return View(results);
        }
    }

    public class SearchResultViewModel
    {
        public List<Resource> Resources { get; set; } = new();
        public List<Accession> Accessions { get; set; } = new();
        public List<DigitalObject> DigitalObjects { get; set; } = new();
    }
}
