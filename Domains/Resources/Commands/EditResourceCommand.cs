using System.Collections.Generic;
using ArchivesSpaceWeb.Domains.Resources.Entities;
using ArchivesSpaceWeb.Domains.Shared.Interfaces;

namespace ArchivesSpaceWeb.Domains.Resources.Commands
{
    public class EditResourceCommand : ICommand<Resource>
    {
        public int Id { get; }
        public Resource Resource { get; }
        public CollectionManagement? CollectionManagement { get; }
        public List<RightsStatement>? RightsStatements { get; }
        public string? ModifiedBy { get; }

        public EditResourceCommand(
            int id, 
            Resource resource, 
            CollectionManagement? collectionManagement, 
            List<RightsStatement>? rightsStatements, 
            string? modifiedBy)
        {
            Id = id;
            Resource = resource;
            CollectionManagement = collectionManagement;
            RightsStatements = rightsStatements;
            ModifiedBy = modifiedBy;
        }
    }
}
