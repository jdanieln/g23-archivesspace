using System.Collections.Generic;
using System.Threading.Tasks;
using ArchivesSpaceWeb.Domains.Admin.Entities;

namespace ArchivesSpaceWeb.Domains.Admin.Interfaces
{
    public interface IEnumService
    {
        Task<List<EnumList>> GetEnumListsAsync();
        Task AddEnumValueAsync(int enumListId, string value, string label);
        Task<int> BulkUpdateEnumValuesAsync(int enumListId, string oldValue, string newValue);
    }
}
