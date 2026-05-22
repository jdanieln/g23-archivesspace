using System;
using System.ComponentModel.DataAnnotations;

namespace ArchivesSpaceWeb.Domains.Agents.Entities
{
    public class Agent
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Type { get; set; } = "Person"; // Person, Family, Corporate

        [Required]
        [StringLength(250)]
        public string Name { get; set; } = string.Empty;

        public string? Source { get; set; } // US 50 (Name Form Source)
        public string? AuthorityId { get; set; } // EAC-CPF metadata

        public string? EacCpfXml { get; set; } // Static backup or imported XML details
    }
}
