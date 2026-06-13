using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace SmartAuditX.ViewModels.CMS
{
    public class AboutUsVM
    {
        public int AboutUsId { get; set; }
        [Required, MaxLength(200)]
        public string Title { get; set; } = string.Empty;
        [Required, MaxLength(500)]
        public string ShortDescription { get; set; } = string.Empty;
        [Required]
        public string FullDescription { get; set; } = string.Empty;
        public string? Mission { get; set; }
        public string? Vision { get; set; }
        
        public string? ImageUrl { get; set; }
        
        public IFormFile? ImageFile { get; set; }
        
        public bool IsActive { get; set; } = true;
    }
}
