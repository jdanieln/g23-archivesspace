using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ArchivesSpaceWeb.Domains.Admin.Queries;
using ArchivesSpaceWeb.Domains.Shared.Interfaces;
using System.Threading.Tasks;

namespace ArchivesSpaceWeb.Domains.Admin.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly IQueryHandler<GetDashboardStatsQuery, DashboardStatsResult> _statsQueryHandler;
        private readonly IQueryHandler<SearchDashboardQuery, SearchDashboardResult> _searchQueryHandler;
        private readonly IQueryHandler<GetUsersListQuery, UsersListResult> _usersListQueryHandler;

        public DashboardController(
            IQueryHandler<GetDashboardStatsQuery, DashboardStatsResult> statsQueryHandler,
            IQueryHandler<SearchDashboardQuery, SearchDashboardResult> searchQueryHandler,
            IQueryHandler<GetUsersListQuery, UsersListResult> usersListQueryHandler)
        {
            _statsQueryHandler = statsQueryHandler;
            _searchQueryHandler = searchQueryHandler;
            _usersListQueryHandler = usersListQueryHandler;
        }

        public async Task<IActionResult> Index()
        {
            var repClaim = User.FindFirst("RepositoryId")?.Value;
            var query = new GetDashboardStatsQuery(repClaim);
            var result = await _statsQueryHandler.HandleAsync(query);

            ViewBag.RepositoryCount = result.RepositoryCount;
            ViewBag.ResourceCount = result.ResourceCount;
            ViewBag.AccessionCount = result.AccessionCount;
            ViewBag.AgentCount = result.AgentCount;
            ViewBag.SubjectCount = result.SubjectCount;
            ViewBag.DigitalObjectCount = result.DigitalObjectCount;
            ViewBag.ActiveRepositoryName = result.ActiveRepositoryName;

            return View(result.RecentEvents);
        }

        [HttpGet]
        public async Task<IActionResult> Search(int? repositoryId, string query)
        {
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

            var reposResult = await _usersListQueryHandler.HandleAsync(new GetUsersListQuery());
            ViewBag.Repositories = reposResult.Repositories;

            if (string.IsNullOrEmpty(query))
            {
                return View(new SearchResultViewModel());
            }

            var searchQuery = new SearchDashboardQuery(repositoryId, query);
            var searchResult = await _searchQueryHandler.HandleAsync(searchQuery);

            var viewModel = new SearchResultViewModel
            {
                Resources = searchResult.Resources,
                Accessions = searchResult.Accessions,
                DigitalObjects = searchResult.DigitalObjects
            };

            return View(viewModel);
        }
    }

    public class SearchResultViewModel
    {
        public System.Collections.Generic.List<ArchivesSpaceWeb.Domains.Resources.Entities.Resource> Resources { get; set; } = new();
        public System.Collections.Generic.List<ArchivesSpaceWeb.Domains.Accessions.Entities.Accession> Accessions { get; set; } = new();
        public System.Collections.Generic.List<ArchivesSpaceWeb.Domains.Resources.Entities.DigitalObject> DigitalObjects { get; set; } = new();
    }
}
