using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ArchivesSpaceWeb.Domains.Admin.Entities;
using ArchivesSpaceWeb.Domains.Resources.Entities;
using ArchivesSpaceWeb.Domains.Resources.Interfaces;
using ArchivesSpaceWeb.Domains.Accessions.Entities;
using ArchivesSpaceWeb.Domains.Accessions.Interfaces;
using ArchivesSpaceWeb.Domains.Agents.Entities;
using ArchivesSpaceWeb.Domains.Shared.Interfaces;

namespace ArchivesSpaceWeb.Domains.Admin.Queries
{
    public class GetDashboardStatsQueryHandler : IQueryHandler<GetDashboardStatsQuery, DashboardStatsResult>
    {
        private readonly IRepository<Repository> _repositoryRepo;
        private readonly IResourceRepository _resourceRepository;
        private readonly IAccessionRepository _accessionRepository;
        private readonly IRepository<Agent> _agentRepo;
        private readonly IRepository<Subject> _subjectRepo;
        private readonly IRepository<DigitalObject> _digitalObjectRepo;
        private readonly IRepository<Event> _eventRepo;

        public GetDashboardStatsQueryHandler(
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

        public async Task<DashboardStatsResult> HandleAsync(GetDashboardStatsQuery query)
        {
            var repositories = await _repositoryRepo.GetAllAsync();
            var resources = await _resourceRepository.GetAllAsync();
            var accessions = await _accessionRepository.GetAllAsync();
            var agents = await _agentRepo.GetAllAsync();
            var subjects = await _subjectRepo.GetAllAsync();
            var digitalObjects = await _digitalObjectRepo.GetAllAsync();

            var allEvents = await _eventRepo.GetAllAsync();
            var recentEvents = allEvents
                .OrderByDescending(e => e.EventDate)
                .Take(6)
                .ToList();

            string activeRepoName = "Ninguno Seleccionado (Administrador del Sistema)";
            if (int.TryParse(query.RepositoryIdClaim, out int repId) && repId > 0)
            {
                var activeRepo = await _repositoryRepo.GetByIdAsync(repId);
                activeRepoName = activeRepo?.Name ?? "Ninguno Seleccionado";
            }

            return new DashboardStatsResult
            {
                RepositoryCount = repositories.Count,
                ResourceCount = resources.Count,
                AccessionCount = accessions.Count,
                AgentCount = agents.Count,
                SubjectCount = subjects.Count,
                DigitalObjectCount = digitalObjects.Count,
                RecentEvents = recentEvents,
                ActiveRepositoryName = activeRepoName
            };
        }
    }
}
