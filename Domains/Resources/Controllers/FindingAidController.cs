using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ArchivesSpaceWeb.Domains.Resources.Entities;
using ArchivesSpaceWeb.Domains.Resources.Queries;
using ArchivesSpaceWeb.Domains.Shared.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ArchivesSpaceWeb.Domains.Resources.Controllers
{
    [AllowAnonymous] // Finding Aid is public for researchers (US 1)
    public class FindingAidController : Controller
    {
        private readonly IQueryHandler<SearchFindingAidsQuery, List<Resource>> _searchQueryHandler;
        private readonly IQueryHandler<GetResourceDetailsQuery, ResourceDetailsResult?> _detailsQueryHandler;
 
        public FindingAidController(
            IQueryHandler<SearchFindingAidsQuery, List<Resource>> searchQueryHandler,
            IQueryHandler<GetResourceDetailsQuery, ResourceDetailsResult?> detailsQueryHandler)
        {
            _searchQueryHandler = searchQueryHandler;
            _detailsQueryHandler = detailsQueryHandler;
        }

        public async Task<IActionResult> Index(string search)
        {
            var query = new SearchFindingAidsQuery(search);
            var resources = await _searchQueryHandler.HandleAsync(query);
 
            ViewBag.Search = search;
            return View(resources);
        }

        public async Task<IActionResult> View(int id)
        {
            var query = new GetResourceDetailsQuery(id);
            var result = await _detailsQueryHandler.HandleAsync(query);
 
            if (result == null) return NotFound();
 
            // Load components hierarchy recursively in order
            ViewBag.Components = result.Components;
 
            // Load related subjects
            ViewBag.Subjects = result.Subjects;
 
            // Load related agents
            ViewBag.Agents = result.Agents;
 
            // Load collection management subrecord details
            ViewBag.CollectionManagement = result.CollectionManagement;
 
            // Load linked rights statements
            ViewBag.RightsStatements = result.RightsStatements;
 
            return View(result.Resource);
        }
    }
}
