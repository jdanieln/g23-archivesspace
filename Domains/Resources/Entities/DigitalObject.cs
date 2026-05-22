using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ArchivesSpaceWeb.Domains.Admin.Entities;

namespace ArchivesSpaceWeb.Domains.Resources.Entities
{
    public class DigitalObject
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
        [StringLength(500)]
        public string FileUri { get; set; } = string.Empty;

        public int? ParentId { get; set; }

        [ForeignKey("ParentId")]
        public virtual DigitalObject? Parent { get; set; }

        public int Position { get; set; }

        [Required]
        public int RepositoryId { get; set; }

        [ForeignKey("RepositoryId")]
        public virtual Repository? Repository { get; set; }
    }
}
