using System.ComponentModel.DataAnnotations;

namespace SmartAuditX.Models.CMS
{
    public class TeamMember
    {
        [Key]
        public int TeamMemberId { get; set; }
        [Required, MaxLength(100)]
        public string FullName { get; set; } = string.Empty;
        [Required, MaxLength(100)]
        public string Designation { get; set; } = string.Empty;
        [MaxLength(500)]
        public string? Bio { get; set; }
        [MaxLength(500)]
        public string? ProfileImageUrl { get; set; }
        [MaxLength(255)]
        public string? LinkedInUrl { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
