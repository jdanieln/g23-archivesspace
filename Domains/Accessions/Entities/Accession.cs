using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ArchivesSpaceWeb.Domains.Admin.Entities;

namespace ArchivesSpaceWeb.Domains.Accessions.Entities
{
    public class Accession
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(250)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Identifier { get; set; } = string.Empty;

        [Required]
        public string AccessionDate { get; set; } = string.Empty;

        public string Dates { get; set; } = string.Empty;
        public string Extents { get; set; } = string.Empty;

        public string? SourceRecordId { get; set; }
        public string? SourceSystem { get; set; }

        [Required]
        public int RepositoryId { get; set; }

        [ForeignKey("RepositoryId")]
        public virtual Repository? Repository { get; set; }
    }
}
