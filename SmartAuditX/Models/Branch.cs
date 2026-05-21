using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartAuditX.Models
{
    public class Branch
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int BranchId { get; set; }

        [Required]
        public int CompanyId { get; set; }

        [Required]
        [MaxLength(200)]
        public string BranchName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string BranchCode { get; set; } = string.Empty;

        [MaxLength(255)]
        [EmailAddress]
        public string? Email { get; set; }

        [MaxLength(50)]
        public string? PhoneNumber { get; set; }

        [MaxLength(500)]
        public string? PhysicalAddress { get; set; }

        public bool IsHeadOffice { get; set; } = false;

        public bool IsActive { get; set; } = true;

        public bool IsDeleted { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Navigation
        //public virtual required Company Company { get; set; } //we can make it required if we want to ensure that every branch must be associated with a company, but for now we will keep it optional to allow for flexibility in case we want to create branches before associating them with a company
        public virtual Company Company { get; set; }
        //added this 
        public ICollection<BranchDepartment> BranchDepartments { get; set; }

    }
}