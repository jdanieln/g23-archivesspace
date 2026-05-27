using ArchivesSpaceWeb.Domains.Shared.Interfaces;

namespace ArchivesSpaceWeb.Domains.Admin.Commands
{
    public class BulkUpdateEnumCommand : ICommand<int>
    {
        public int EnumListId { get; }
        public string OldValue { get; }
        public string NewValue { get; }

        public BulkUpdateEnumCommand(int enumListId, string oldValue, string newValue)
        {
            EnumListId = enumListId;
            OldValue = oldValue;
            NewValue = newValue;
        }
    }
}
