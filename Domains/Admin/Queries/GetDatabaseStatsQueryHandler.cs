using System.Threading.Tasks;
using ArchivesSpaceWeb.Domains.Admin.Interfaces;
using ArchivesSpaceWeb.Domains.Shared.Interfaces;

namespace ArchivesSpaceWeb.Domains.Admin.Queries
{
    public class GetDatabaseStatsQueryHandler : IQueryHandler<GetDatabaseStatsQuery, DatabaseStatsResult>
    {
        private readonly IBackupService _backupService;

        public GetDatabaseStatsQueryHandler(IBackupService backupService)
        {
            _backupService = backupService;
        }

        public async Task<DatabaseStatsResult> HandleAsync(GetDatabaseStatsQuery query)
        {
            await Task.Yield();
            return new DatabaseStatsResult
            {
                ConnectionString = _backupService.GetConnectionString(),
                DatabasePath = _backupService.GetDatabasePath(),
                DatabaseExists = _backupService.DatabaseExists(),
                DatabaseSize = _backupService.GetDatabaseSize(),
                LastModified = _backupService.GetLastModified(),
                BackupFiles = _backupService.GetBackupFiles()
            };
        }
    }
}
