using System.Threading.Tasks;
using ArchivesSpaceWeb.Domains.Admin.Interfaces;
using ArchivesSpaceWeb.Domains.Shared.Interfaces;

namespace ArchivesSpaceWeb.Domains.Admin.Commands
{
    public class BackupDatabaseCommandHandler : ICommandHandler<BackupDatabaseCommand, string>
    {
        private readonly IBackupService _backupService;

        public BackupDatabaseCommandHandler(IBackupService backupService)
        {
            _backupService = backupService;
        }

        public async Task<string> HandleAsync(BackupDatabaseCommand command)
        {
            return await _backupService.BackupDatabaseAsync();
        }
    }
}
