using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ArchivesSpaceWeb.Domains.Agents.Entities;
using ArchivesSpaceWeb.Domains.Agents.Interfaces;
using ArchivesSpaceWeb.Domains.Agents.Queries;
using ArchivesSpaceWeb.Domains.Agents.Commands;
using ArchivesSpaceWeb.Domains.Shared.Interfaces;
using ArchivesSpaceWeb.Domains.Admin.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ArchivesSpaceWeb.Domains.Agents.Controllers
{
    [Authorize]
    public class AgentController : Controller
    {
        private readonly IQueryHandler<GetAgentsListQuery, List<Agent>> _listQueryHandler;
        private readonly IQueryHandler<GetAgentDetailsQuery, AgentDetailsResult?> _detailsQueryHandler;
        private readonly IQueryHandler<ExportEacCpfQuery, XDocument?> _exportQueryHandler;
        private readonly ICommandHandler<CreateAgentCommand, Agent> _createCommandHandler;
        private readonly ICommandHandler<EditAgentCommand, Agent> _editCommandHandler;

        public AgentController(
            IQueryHandler<GetAgentsListQuery, List<Agent>> listQueryHandler,
            IQueryHandler<GetAgentDetailsQuery, AgentDetailsResult?> detailsQueryHandler,
            IQueryHandler<ExportEacCpfQuery, XDocument?> exportQueryHandler,
            ICommandHandler<CreateAgentCommand, Agent> createCommandHandler,
            ICommandHandler<EditAgentCommand, Agent> editCommandHandler)
        {
            _listQueryHandler = listQueryHandler;
            _detailsQueryHandler = detailsQueryHandler;
            _exportQueryHandler = exportQueryHandler;
            _createCommandHandler = createCommandHandler;
            _editCommandHandler = editCommandHandler;
        }

        public async Task<IActionResult> Index()
        {
            var query = new GetAgentsListQuery();
            var agents = await _listQueryHandler.HandleAsync(query);
            return View(agents);
        }

        public async Task<IActionResult> Details(int id)
        {
            var query = new GetAgentDetailsQuery(id);
            var result = await _detailsQueryHandler.HandleAsync(query);

            if (result == null) return NotFound();

            ViewBag.Resources = result.Resources;
            ViewBag.Accessions = result.Accessions;

            return View(result.Agent);
        }

        [HttpGet]
        [Authorize(Roles = "SystemAdmin,RepositoryManager,BasicDataEntry")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SystemAdmin,RepositoryManager,BasicDataEntry")]
        public async Task<IActionResult> Create(Agent agent)
        {
            if (ModelState.IsValid)
            {
                var command = new CreateAgentCommand(agent, User.Identity?.Name);
                await _createCommandHandler.HandleAsync(command);
                return RedirectToAction(nameof(Index));
            }

            return View(agent);
        }

        [HttpGet]
        [Authorize(Roles = "SystemAdmin,RepositoryManager")]
        public async Task<IActionResult> Edit(int id)
        {
            var query = new GetAgentDetailsQuery(id);
            var result = await _detailsQueryHandler.HandleAsync(query);
            if (result == null) return NotFound();

            return View(result.Agent);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SystemAdmin,RepositoryManager")]
        public async Task<IActionResult> Edit(int id, Agent agent)
        {
            if (id != agent.Id) return NotFound();

            if (ModelState.IsValid)
            {
                var command = new EditAgentCommand(agent, User.Identity?.Name);
                await _editCommandHandler.HandleAsync(command);
                
                return RedirectToAction(nameof(Details), new { id = agent.Id });
            }

            return View(agent);
        }

        // US 19: Export agent records as EAC-CPF
        [HttpGet]
        public async Task<IActionResult> ExportEacCpf(int id)
        {
            var query = new ExportEacCpfQuery(id);
            var doc = await _exportQueryHandler.HandleAsync(query);

            if (doc == null) return NotFound();

            var stream = new MemoryStream();
            using (var writer = new StreamWriter(stream, Encoding.UTF8))
            {
                doc.Save(writer);
            }
            stream.Position = 0;

            // Retrieve agent details for filename
            var detailsQuery = new GetAgentDetailsQuery(id);
            var details = await _detailsQueryHandler.HandleAsync(detailsQuery);
            string agentName = details?.Agent.Name ?? "agent";

            string fileName = $"EAC-CPF-{agentName.Replace(" ", "-")}.xml";
            return File(stream, "application/xml", fileName);
        }
    }
}
