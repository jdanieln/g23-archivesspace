using System.ComponentModel.DataAnnotations;
using ArchivesSpaceWeb.Domains.Agents.Entities;

namespace ArchivesSpaceWeb.Domains.Accessions.Entities
{
    public class AccessionAgent
    {
        public int AccessionId { get; set; }
        public virtual Accession? Accession { get; set; }

        public int AgentId { get; set; }
        public virtual Agent? Agent { get; set; }

        [Required]
        [StringLength(50)]
        public string Role { get; set; } = "Creator"; // Creator, Source, Subject
    }
}
