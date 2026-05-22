using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArchivesSpaceWeb.Domains.Admin.Entities
{
    public class EnumValue
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int EnumListId { get; set; }

        [ForeignKey("EnumListId")]
        public virtual EnumList? EnumList { get; set; }

        [Required]
        [StringLength(100)]
        public string Value { get; set; } = string.Empty;

        [Required]
        [StringLength(150)]
        public string Label { get; set; } = string.Empty;
    }
}
