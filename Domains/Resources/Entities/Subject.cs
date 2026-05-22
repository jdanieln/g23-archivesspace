using System.ComponentModel.DataAnnotations;

namespace ArchivesSpaceWeb.Domains.Resources.Entities
{
    public class Subject
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(250)]
        public string Heading { get; set; } = string.Empty;

        [StringLength(50)]
        public string Source { get; set; } = "local";

        [StringLength(150)]
        public string? StandardIdentifier { get; set; } // US 8
    }
}
