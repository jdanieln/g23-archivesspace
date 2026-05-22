using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArchivesSpaceWeb.Domains.Resources.Entities
{
    public class ArchivalObject
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(250)]
        public string Title { get; set; } = string.Empty;

        [StringLength(100)]
        public string? ComponentIdentifier { get; set; } // US 5

        [Required]
        [StringLength(50)]
        public string LevelOfDescription { get; set; } = "Item";

        public string? LevelOfDescriptionOther { get; set; }

        public string Dates { get; set; } = string.Empty;
        public string Extents { get; set; } = string.Empty;

        public int? ParentId { get; set; }

        [ForeignKey("ParentId")]
        public virtual ArchivalObject? Parent { get; set; }

        [Required]
        public int ResourceId { get; set; }

        [ForeignKey("ResourceId")]
        public virtual Resource? Resource { get; set; }

        public int Position { get; set; } // Ordering index in siblings list
    }
}
