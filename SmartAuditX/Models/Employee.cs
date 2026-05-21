using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartAuditX.Models
{
    public class Employee
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int EmployeeId { get; set; }

        [Required]
        public int CompanyId { get; set; }

        public int? BranchId { get; set; }

        public int? DepartmentId { get; set; }

        public int? DesignationId { get; set; }

        [Required]
        [MaxLength(50)]
        public string EmployeeCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? LastName { get; set; }

        [Required]
        public string Gender { get; set; } =string.Empty;

        public DateTime? DateOfBirth { get; set; }

        [MaxLength(30)]
        public string? CNICOrNationalId { get; set; }

        [MaxLength(20)]
        public string? PersonalPhone { get; set; }

        [MaxLength(255)]
        public string? PersonalEmail { get; set; }

        [Required]
        public DateTime JoiningDate { get; set; }

        [MaxLength(500)]
        public string? ProfileImageUrl { get; set; }

        public bool IsSystemUser { get; set; } = false;

        public bool IsActive { get; set; } = true;

        public bool IsDeleted { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Navigation
        public Company? Company { get; set; }
        public Branch? Branch { get; set; }
        public Department? Department { get; set; }
        public Designation? Designation { get; set; }

        public ApplicationUser? User { get; set; }
    }
}