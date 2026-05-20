using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartAuditX.Models
{
    public class CompanyContact
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CompanyContactId { get; set; }

        [Required]
        public int CompanyId { get; set; }

        [MaxLength(150)]
        public string? ContactName { get; set; }

        [Required]
        [MaxLength(255)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string PhoneNumber { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? FaxNumber { get; set; }

        [MaxLength(500)]
        public string? PhysicalAddress { get; set; }

        public bool IsPrimary { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Navigation
        //public virtual required Company Company { get; set; } //we can make it required if we want to ensure that every contact must be associated with a company, but for now we will keep it optional to allow for flexibility in case we want to create contacts before associating them with a company

        public virtual Company Company { get; set; }



    }
}