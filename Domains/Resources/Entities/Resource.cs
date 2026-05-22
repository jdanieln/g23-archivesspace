using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ArchivesSpaceWeb.Domains.Admin.Entities;

namespace ArchivesSpaceWeb.Domains.Resources.Entities
{
    public class Resource
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(250)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Identifier { get; set; } = string.Empty; // Unique resource ID

        [Required]
        [StringLength(50)]
        public string LevelOfDescription { get; set; } = "Collection"; // Predefined list or "Other"

        public string? LevelOfDescriptionOther { get; set; }

        [Required]
        [StringLength(100)]
        public string Dates { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Extents { get; set; } = string.Empty;

        public string? LanguageOfDescription { get; set; }
        public string? FindingAidAuthor { get; set; }
        public string? FindingAidSponsor { get; set; }
        public string? FindingAidNote { get; set; }

        // Migration testing source info (US 7)
        public string? SourceRecordId { get; set; }
        public string? SourceSystem { get; set; }

        [Required]
        public int RepositoryId { get; set; }

        [ForeignKey("RepositoryId")]
        public virtual Repository? Repository { get; set; }

        [Timestamp]
        public byte[]? RowVersion { get; set; } // Optimistic locking
    }
}
