using System.Collections.Generic;
using ArchivesSpaceWeb.Domains.Shared.Interfaces;

namespace ArchivesSpaceWeb.Domains.Resources.Commands
{
    public class UpdateResourceHierarchyCommand : ICommand<bool>
    {
        public List<HierarchyUpdateModel> Updates { get; }

        public UpdateResourceHierarchyCommand(List<HierarchyUpdateModel> updates)
        {
            Updates = updates;
        }
    }

    public class HierarchyUpdateModel
    {
        public int Id { get; set; }
        public int? ParentId { get; set; }
        public int Position { get; set; }
    }
}
