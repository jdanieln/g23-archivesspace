using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ArchivesSpaceWeb.Domains.Admin.Entities
{
    public class EnumList
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty; // e.g. level_of_description, agent_role, instance_type

        public virtual ICollection<EnumValue> EnumValues { get; set; } = new List<EnumValue>();
    }
}
