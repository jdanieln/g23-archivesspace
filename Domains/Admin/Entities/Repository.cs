using System;
using System.ComponentModel.DataAnnotations;

namespace ArchivesSpaceWeb.Domains.Admin.Entities
{
    public class Repository
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string Code { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        // Dynamic properties stored as JSON (US 23)
        public string AdditionalPropertiesJson { get; set; } = "{}";
    }
}
