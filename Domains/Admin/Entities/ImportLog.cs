using System;
using System.ComponentModel.DataAnnotations;

namespace ArchivesSpaceWeb.Domains.Admin.Entities
{
    public class ImportLog
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string ImporterName { get; set; } = string.Empty; // EAD, EAC-CPF, MARCXML, CSV

        [Required]
        public DateTime Timestamp { get; set; } = DateTime.Now;

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Success"; // Success, Failed

        public string Details { get; set; } = string.Empty;
        public string ErrorLog { get; set; } = string.Empty;
    }
}
