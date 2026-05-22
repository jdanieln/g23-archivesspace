using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArchivesSpaceWeb.Domains.Resources.Entities
{
    public class Container
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Indicator { get; set; } = string.Empty; // Box 1, Folder 3

        [Required]
        [StringLength(50)]
        public string ContainerType { get; set; } = "Box";

        [Required]
        public int InstanceId { get; set; }
        [ForeignKey("InstanceId")]
        public virtual Instance? Instance { get; set; }

        public int? LocationId { get; set; }
        [ForeignKey("LocationId")]
        public virtual Location? Location { get; set; }
    }
}
