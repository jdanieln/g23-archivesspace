using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArchivesSpaceWeb.Domains.Resources.Entities
{
    public class Instance
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string InstanceType { get; set; } = "Text"; // Text, Graphic Materials, Audio

        public int? ArchivalObjectId { get; set; }
        [ForeignKey("ArchivalObjectId")]
        public virtual ArchivalObject? ArchivalObject { get; set; }

        public int? ResourceId { get; set; }
        [ForeignKey("ResourceId")]
        public virtual Resource? Resource { get; set; }
    }
}
