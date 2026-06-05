using System.ComponentModel.DataAnnotations;

namespace SmartAuditX.Models.ViewModels
{
    public class DepartmentViewModel
    {
        public int? DepartmentId { get; set; }

        [Required(ErrorMessage = "Department name is required.")]
        [MaxLength(150)]
        [Display(Name = "Department Name")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Department code is required.")]
        [MaxLength(50)]
        [RegularExpression(@"^[A-Za-z0-9_-]+$", ErrorMessage = "Code may only contain letters, numbers, hyphens, and underscores.")]
        [Display(Name = "Department Code")]
        public string Code { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;
    }
}