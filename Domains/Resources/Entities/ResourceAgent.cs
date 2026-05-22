using System.ComponentModel.DataAnnotations;
using ArchivesSpaceWeb.Domains.Agents.Entities;

namespace ArchivesSpaceWeb.Domains.Resources.Entities
{
    // Association Table for Agent Relationship (US 35)
    public class ResourceAgent
    {
        public int ResourceId { get; set; }
        public virtual Resource? Resource { get; set; }

        public int AgentId { get; set; }
        public virtual Agent? Agent { get; set; }

        [Required]
        [StringLength(50)]
        public string Role { get; set; } = "Creator"; // Creator, Source, Subject
    }
}
