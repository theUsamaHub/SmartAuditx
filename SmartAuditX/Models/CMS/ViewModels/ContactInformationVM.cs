using System.ComponentModel.DataAnnotations;

namespace SmartAuditX.Models.CMS.ViewModels.CMS
{
    public class ContactInformationVM
    {
        public int ContactInformationId { get; set; }
        [Required, MaxLength(200)]
        public string CompanyName { get; set; } = string.Empty; 
        [MaxLength(100)]
        public string? SupportEmail { get; set; }
        [MaxLength(100)]
        public string? SalesEmail { get; set; }
        [MaxLength(50)]
        public string? PhoneNumber { get; set; }
        [MaxLength(500)]
        public string? Address { get; set; }
        [MaxLength(1000)]
        public string? GoogleMapEmbedUrl { get; set; } 
        [MaxLength(255)]
        public string? FacebookUrl { get; set; }
        [MaxLength(255)]
        public string? LinkedInUrl { get; set; }
        [MaxLength(255)]
        public string? TwitterUrl { get; set; }
        [MaxLength(255)]
        public string? InstagramUrl { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
