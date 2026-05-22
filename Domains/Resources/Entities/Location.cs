using System.ComponentModel.DataAnnotations;

namespace ArchivesSpaceWeb.Domains.Resources.Entities
{
    public class Location
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Coordinate { get; set; } = string.Empty; // Room 204, Row 12

        public string Room { get; set; } = string.Empty;
        public string Shelf { get; set; } = string.Empty;
    }
}
