using System.ComponentModel.DataAnnotations;

namespace SmartAuditX.Models.ViewModels
{
    public class BranchViewModel
    {
        public int? BranchId { get; set; }

        [Required(ErrorMessage = "Branch name is required.")]
        [MaxLength(200)]
        [Display(Name = "Branch Name")]
        public string BranchName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Branch code is required.")]
        [MaxLength(50)]
        [RegularExpression(@"^[A-Za-z0-9_-]+$", ErrorMessage = "Code may only contain letters, numbers, hyphens, and underscores.")]
        [Display(Name = "Branch Code")]
        public string BranchCode { get; set; } = string.Empty;

        [MaxLength(255)]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        public string? Email { get; set; }

        [MaxLength(50)]
        [Display(Name = "Phone Number")]
        public string? PhoneNumber { get; set; }

        [MaxLength(500)]
        [Display(Name = "Physical Address")]
        public string? PhysicalAddress { get; set; }

        [Display(Name = "Head Office")]
        public bool IsHeadOffice { get; set; } = false;

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;
    }
}
