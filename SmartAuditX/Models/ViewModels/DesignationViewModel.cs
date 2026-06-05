using System.ComponentModel.DataAnnotations;

namespace SmartAuditX.Models.ViewModels
{
    public class DesignationViewModel
    {
        public int? DesignationId { get; set; }

        [Required(ErrorMessage = "Designation name is required.")]
        [MaxLength(150)]
        [Display(Name = "Designation Name")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Designation code is required.")]
        [MaxLength(50)]
        [RegularExpression(@"^[A-Za-z0-9_-]+$", ErrorMessage = "Code may only contain letters, numbers, hyphens, and underscores.")]
        [Display(Name = "Designation Code")]
        public string Code { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;
    }
}
