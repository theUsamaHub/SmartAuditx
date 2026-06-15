using System.ComponentModel.DataAnnotations;

namespace SmartAuditX.Models.CMS.ViewModels.CMS
{
    public class HeroSectionVM
    {
        public int HeroSectionId { get; set; }
        [MaxLength(100)]
        public string? BadgeText { get; set; }
        [Required, MaxLength(200)]
        public string Title { get; set; } = string.Empty;
        [Required, MaxLength(500)]
        public string Description { get; set; } = string.Empty;
        [MaxLength(50)]
        public string? PrimaryButtonText { get; set; }
        [MaxLength(255)]
        public string? PrimaryButtonUrl { get; set; }
        [MaxLength(50)]
        public string? SecondaryButtonText { get; set; }
        [MaxLength(255)]
        public string? SecondaryButtonUrl { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
