using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ArchivesSpaceWeb.Domains.Admin.Entities;

namespace ArchivesSpaceWeb.Domains.Identity.Entities
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        [StringLength(30)]
        public string Role { get; set; } = "ReadOnly"; // SystemAdmin, RepositoryManager, AdvancedDataEntry, BasicDataEntry, ReadOnly

        [Required]
        [StringLength(20)]
        public string AuthMode { get; set; } = "Local"; // Local, LDAP

        public int? RepositoryId { get; set; }
        
        [ForeignKey("RepositoryId")]
        public virtual Repository? Repository { get; set; }
    }
}
