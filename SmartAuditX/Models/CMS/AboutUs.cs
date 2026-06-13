using System.ComponentModel.DataAnnotations;

namespace SmartAuditX.Models.CMS
{
    public class AboutUs
    {
        [Key]
        public int AboutUsId { get; set; }
        [Required, MaxLength(200)]
        public string Title { get; set; } = string.Empty;
        [Required, MaxLength(500)]
        public string ShortDescription { get; set; } = string.Empty;
        [Required]
        public string FullDescription { get; set; } = string.Empty;
        public string? Mission { get; set; }
        public string? Vision { get; set; }
        [MaxLength(500)]
        public string? ImageUrl { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
