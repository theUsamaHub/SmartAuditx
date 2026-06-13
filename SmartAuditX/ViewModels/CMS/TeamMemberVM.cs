using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace SmartAuditX.ViewModels.CMS
{
    public class TeamMemberVM
    {
        public int TeamMemberId { get; set; }
        [Required, MaxLength(100)]
        public string FullName { get; set; } = string.Empty;
        [Required, MaxLength(100)]
        public string Designation { get; set; } = string.Empty;
        [MaxLength(500)]
        public string? Bio { get; set; }
        
        public string? ProfileImageUrl { get; set; }
        
        public IFormFile? ProfileImageFile { get; set; }
        
        [MaxLength(255)]
        public string? LinkedInUrl { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
