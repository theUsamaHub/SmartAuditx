using System.ComponentModel.DataAnnotations;

namespace SmartAuditX.Models.ViewModels
{
    public class EmployeeViewModel
    {
        public int? EmployeeId { get; set; }

        // Section A: Basic Info
        [Required(ErrorMessage = "First name is required.")]
        [MaxLength(100)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        [MaxLength(100)]
        [Display(Name = "Last Name")]
        public string? LastName { get; set; }

        [Required(ErrorMessage = "Gender is required.")]
        public string Gender { get; set; } = string.Empty;

        [Display(Name = "Date of Birth")]
        public DateTime? DateOfBirth { get; set; }

        [Display(Name = "CNIC / National ID")]
        [MaxLength(30)]
        public string? CNICOrNationalId { get; set; }

        [MaxLength(255)]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        [Display(Name = "Personal Email")]
        public string? PersonalEmail { get; set; }

        [MaxLength(20)]
        [Display(Name = "Personal Phone")]
        public string? PersonalPhone { get; set; }

        // Section B: Organization
        [Display(Name = "Branch")]
        public int? BranchId { get; set; }

        [Display(Name = "Department")]
        public int? DepartmentId { get; set; }

        [Display(Name = "Designation")]
        public int? DesignationId { get; set; }

        [Display(Name = "Joining Date")]
        [Required(ErrorMessage = "Joining date is required.")]
        public DateTime JoiningDate { get; set; } = DateTime.Today;

        // Section C: System User Option
        [Display(Name = "Create System User Account")]
        public bool IsSystemUser { get; set; } = false;

        // Identity User Fields (shown only if IsSystemUser = true)
        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;

        // Identity Fields
        [Display(Name = "System Email")]
        [MaxLength(256)]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        public string? SystemEmail { get; set; }

        [Display(Name = "Password")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters.")]
        public string? Password { get; set; }

        [Display(Name = "Confirm Password")]
        [Compare("Password", ErrorMessage = "Password and confirmation password do not match.")]
        public string? ConfirmPassword { get; set; }

        [Display(Name = "Role")]
        public string? Role { get; set; }

        // Read-only field for display
        public string? EmployeeCode { get; set; }
    }
}
