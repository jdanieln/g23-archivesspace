using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using ArchivesSpaceWeb.Domains.Admin.Interfaces;

namespace ArchivesSpaceWeb.Domains.Admin.Infrastructure
{
    public class BackupService : IBackupService
    {
        private readonly IConfiguration _configuration;

        public BackupService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GetConnectionString()
        {
            return _configuration.GetConnectionString("DefaultConnection") ?? "Data Source=archives_space.db";
        }

        public string GetDatabasePath()
        {
            var connStr = GetConnectionString();
            string dbFilePath = "archives_space.db";
            if (connStr.Contains("Data Source="))
            {
                dbFilePath = connStr.Replace("Data Source=", "").Split(';')[0].Trim();
            }
            return Path.GetFullPath(dbFilePath);
        }

        public bool DatabaseExists()
        {
            return File.Exists(GetDatabasePath());
        }

        public string GetDatabaseSize()
        {
            if (!DatabaseExists()) return "N/D";
            var fileInfo = new FileInfo(GetDatabasePath());
            return $"{fileInfo.Length / 1024.0:F2} KB";
        }

        public string GetLastModified()
        {
            if (!DatabaseExists()) return "N/D";
            var fileInfo = new FileInfo(GetDatabasePath());
            return fileInfo.LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss");
        }

        public List<string> GetBackupFiles()
        {
            var backupDir = Path.Combine(Directory.GetCurrentDirectory(), "backups");
            var backupFiles = new List<string>();
            if (Directory.Exists(backupDir))
            {
                backupFiles = Directory.GetFiles(backupDir, "*.db")
                     .Select(f => Path.GetFileName(f))
                     .OrderByDescending(f => f)
                     .ToList();
            }
            return backupFiles;
        }

        public async Task<string> BackupDatabaseAsync()
        {
            var sourcePath = GetDatabasePath();
            if (!File.Exists(sourcePath))
            {
                throw new FileNotFoundException("Error: El archivo de base de datos origen no existe.");
            }

            var backupDir = Path.Combine(Directory.GetCurrentDirectory(), "backups");
            if (!Directory.Exists(backupDir))
            {
                Directory.CreateDirectory(backupDir);
            }

            var timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
            var destFileName = $"archives_space-backup-{timestamp}.db";
            var destPath = Path.Combine(backupDir, destFileName);

            // SQLite is fine to copy directly because the web app isn't performing heavy write traffic during backup.
            // Under ASP.NET Core, we'll perform a standard asynchronous file copy.
            using (var sourceStream = new FileStream(sourcePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var destStream = new FileStream(destPath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                await sourceStream.CopyToAsync(destStream);
            }

            return destFileName;
        }
    }
}
