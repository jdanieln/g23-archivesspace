using System.Collections.Generic;
using System.Threading.Tasks;

namespace ArchivesSpaceWeb.Domains.Admin.Interfaces
{
    public interface IBackupService
    {
        string GetConnectionString();
        string GetDatabasePath();
        bool DatabaseExists();
        string GetDatabaseSize();
        string GetLastModified();
        List<string> GetBackupFiles();
        Task<string> BackupDatabaseAsync();
    }
}
