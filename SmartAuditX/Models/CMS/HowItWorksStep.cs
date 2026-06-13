using System.ComponentModel.DataAnnotations;

namespace SmartAuditX.Models.CMS
{
    public class HowItWorksStep
    {
        [Key]
        public int HowItWorksStepId { get; set; }
        public int StepNumber { get; set; }
        [Required, MaxLength(150)]
        public string Title { get; set; } = string.Empty;
        [Required, MaxLength(500)]
        public string Description { get; set; } = string.Empty;
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
