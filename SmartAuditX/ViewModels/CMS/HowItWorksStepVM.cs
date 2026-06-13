using System.ComponentModel.DataAnnotations;

namespace SmartAuditX.ViewModels.CMS
{
    public class HowItWorksStepVM
    {
        public int HowItWorksStepId { get; set; }
        public int StepNumber { get; set; }
        [Required, MaxLength(150)]
        public string Title { get; set; } = string.Empty;
        [Required, MaxLength(500)]
        public string Description { get; set; } = string.Empty;
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
