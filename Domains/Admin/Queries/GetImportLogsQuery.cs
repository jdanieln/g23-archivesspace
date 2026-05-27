using System.Collections.Generic;
using ArchivesSpaceWeb.Domains.Admin.Entities;
using ArchivesSpaceWeb.Domains.Shared.Interfaces;

namespace ArchivesSpaceWeb.Domains.Admin.Queries
{
    public class GetImportLogsQuery : IQuery<List<ImportLog>>
    {
        public int Count { get; }

        public GetImportLogsQuery(int count)
        {
            Count = count;
        }
    }
}
