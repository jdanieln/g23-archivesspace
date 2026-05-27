using ArchivesSpaceWeb.Domains.Shared.Interfaces;

namespace ArchivesSpaceWeb.Domains.Admin.Commands
{
    public class CreateEnumValueCommand : ICommand<bool>
    {
        public int EnumListId { get; }
        public string Value { get; }
        public string Label { get; }

        public CreateEnumValueCommand(int enumListId, string value, string label)
        {
            EnumListId = enumListId;
            Value = value;
            Label = label;
        }
    }
}
