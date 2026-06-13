using System.ComponentModel.DataAnnotations;

namespace SmartAuditX.Models.CMS
{
    public class Faq
    {
        [Key]
        public int FaqId { get; set; }
        [Required, MaxLength(500)]
        public string Question { get; set; } = string.Empty;
        [Required]
        public string Answer { get; set; } = string.Empty;
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
