using System.ComponentModel.DataAnnotations;

namespace SmartAuditX.Models.CMS.ViewModels.CMS
{
    public class FaqVM
    {
        public int FaqId { get; set; }
        [Required, MaxLength(500)]
        public string Question { get; set; } = string.Empty;
        [Required]
        public string Answer { get; set; } = string.Empty;
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
