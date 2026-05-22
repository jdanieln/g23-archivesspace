using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArchivesSpaceWeb.Domains.Resources.Entities
{
    public class CollectionManagement
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ResourceId { get; set; }

        [ForeignKey("ResourceId")]
        public virtual Resource? Resource { get; set; }

        [Required]
        [StringLength(50)]
        public string ProcessingStatus { get; set; } = "New"; // New, In Progress, Completed

        [Required]
        [StringLength(50)]
        public string ProcessingPriority { get; set; } = "Medium"; // High, Medium, Low

        public string ProcessingNotes { get; set; } = string.Empty;
    }
}
