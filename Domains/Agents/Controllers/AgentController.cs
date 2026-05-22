using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ArchivesSpaceWeb.Domains.Agents.Entities;
using ArchivesSpaceWeb.Domains.Agents.Interfaces;
using ArchivesSpaceWeb.Domains.Shared.Interfaces;
using ArchivesSpaceWeb.Domains.Admin.Entities;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ArchivesSpaceWeb.Domains.Agents.Controllers
{
    [Authorize]
    public class AgentController : Controller
    {
        private readonly IAgentRepository _agentRepository;
        private readonly IRepository<Event> _eventRepository;

        public AgentController(IAgentRepository agentRepository, IRepository<Event> eventRepository)
        {
            _agentRepository = agentRepository;
            _eventRepository = eventRepository;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _agentRepository.GetAllAsync());
        }

        public async Task<IActionResult> Details(int id)
        {
            var agent = await _agentRepository.GetByIdAsync(id);
            if (agent == null) return NotFound();

            // Fetch related resources (US 35)
            ViewBag.Resources = await _agentRepository.GetLinkedResourcesAsync(id);
            ViewBag.Accessions = await _agentRepository.GetLinkedAccessionsAsync(id);

            return View(agent);
        }

        [HttpGet]
        public IActionResult Create()
        {
            if (User.IsInRole("ReadOnly")) return RedirectToAction("AccessDenied", "Account");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Agent agent)
        {
            if (User.IsInRole("ReadOnly")) return RedirectToAction("AccessDenied", "Account");

            if (ModelState.IsValid)
            {
                await _agentRepository.AddAsync(agent);
                await _agentRepository.SaveChangesAsync();

                // Log creation event (US 37)
                var creationEvent = new Event
                {
                    EventType = "Creation",
                    EventDate = DateTime.Now,
                    Description = $"Agente '{agent.Name}' ({agent.Type}) registrado por {User.Identity?.Name}."
                };
                await _eventRepository.AddAsync(creationEvent);
                await _eventRepository.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            return View(agent);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            if (User.IsInRole("ReadOnly") || User.IsInRole("BasicDataEntry"))
                return RedirectToAction("AccessDenied", "Account");

            var agent = await _agentRepository.GetByIdAsync(id);
            if (agent == null) return NotFound();

            return View(agent);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Agent agent)
        {
            if (User.IsInRole("ReadOnly") || User.IsInRole("BasicDataEntry"))
                return RedirectToAction("AccessDenied", "Account");

            if (id != agent.Id) return NotFound();

            if (ModelState.IsValid)
            {
                await _agentRepository.UpdateAsync(agent);

                var modEvent = new Event
                {
                    EventType = "Modification",
                    EventDate = DateTime.Now,
                    Description = $"Agente '{agent.Name}' modificado por {User.Identity?.Name}."
                };
                await _eventRepository.AddAsync(modEvent);
                await _agentRepository.SaveChangesAsync();
                
                return RedirectToAction(nameof(Details), new { id = agent.Id });
            }

            return View(agent);
        }

        // US 19: Export agent records as EAC-CPF
        [HttpGet]
        public async Task<IActionResult> ExportEacCpf(int id)
        {
            var agent = await _agentRepository.GetByIdAsync(id);
            if (agent == null) return NotFound();

            // Build compliant EAC-CPF XML
            var doc = new XDocument(
                new XElement("eac-cpf",
                    new XAttribute("xmlns", "urn:isbn:1-931666-33-4"),
                    new XElement("control",
                        new XElement("recordId", $"agent-{agent.Id}"),
                        new XElement("maintenanceStatus", "derived"),
                        new XElement("maintenanceAgency",
                            new XElement("agencyName", "ArchivesSpaceWeb G23 System")
                        ),
                        new XElement("languageDeclaration",
                            new XElement("language", new XAttribute("languageCode", "spa"), "Español")
                        )
                    ),
                    new XElement("identity",
                        new XElement("entityType", agent.Type.ToLower() == "corporate" ? "corporateBody" : agent.Type.ToLower()),
                        new XElement("nameEntry",
                            new XElement("part", agent.Name),
                            new XElement("authorizedForm", "local")
                        )
                    ),
                    new XElement("description",
                        new XElement("existDates",
                            new XElement("dateRange",
                                new XElement("fromDate", "N/D"),
                                new XElement("toDate", "N/D")
                            )
                        ),
                        new XElement("biogHist",
                            new XElement("p", $"Registro de agente importado/administrado como {agent.Type}. Autoridad origen: {agent.Source ?? "Local"}.")
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

            string fileName = $"EAC-CPF-{agent.Name.Replace(" ", "-")}.xml";
            return File(stream, "application/xml", fileName);
        }
    }
}
