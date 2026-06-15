using System.ComponentModel.DataAnnotations;

namespace SmartAuditX.Models.CMS.ViewModels.CMS
{
    public class SecurityFeatureVM
    {
        public int SecurityFeatureId { get; set; }
        [Required, MaxLength(150)]
        public string Title { get; set; } = string.Empty;
        [Required, MaxLength(500)]
        public string Description { get; set; } = string.Empty;
        [MaxLength(100)]
        public string? IconName { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
