using System;
using System.Collections.Generic;
using ArchivesSpaceWeb.Domains.Shared.Interfaces;

namespace ArchivesSpaceWeb.Domains.Admin.Queries
{
    public class GetDatabaseStatsQuery : IQuery<DatabaseStatsResult>
    {
    }

    public class DatabaseStatsResult
    {
        public string ConnectionString { get; set; } = string.Empty;
        public string DatabasePath { get; set; } = string.Empty;
        public bool DatabaseExists { get; set; }
        public string DatabaseSize { get; set; } = string.Empty;
        public string LastModified { get; set; } = string.Empty;
        public List<string> BackupFiles { get; set; } = new();
    }
}
