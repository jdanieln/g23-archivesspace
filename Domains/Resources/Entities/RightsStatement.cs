using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArchivesSpaceWeb.Domains.Resources.Entities
{
    public class RightsStatement
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string RightsType { get; set; } = "Copyright"; // Copyright, License, Donor Restrictions

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Active";

        public string Description { get; set; } = string.Empty;

        public int? ResourceId { get; set; }
        [ForeignKey("ResourceId")]
        public virtual Resource? Resource { get; set; }
    }
}
