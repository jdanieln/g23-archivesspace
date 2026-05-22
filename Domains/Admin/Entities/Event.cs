using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ArchivesSpaceWeb.Domains.Agents.Entities;
using ArchivesSpaceWeb.Domains.Resources.Entities;

namespace ArchivesSpaceWeb.Domains.Admin.Entities
{
    public class Event
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string EventType { get; set; } = "Creation"; // Accession, Transfer, Digitization, etc.

        [Required]
        public DateTime EventDate { get; set; } = DateTime.Now;

        public string Description { get; set; } = string.Empty;

        public int? AgentId { get; set; }
        [ForeignKey("AgentId")]
        public virtual Agent? Agent { get; set; }

        public int? ResourceId { get; set; }
        [ForeignKey("ResourceId")]
        public virtual Resource? Resource { get; set; }
    }
}
