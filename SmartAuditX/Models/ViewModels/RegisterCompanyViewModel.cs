using System.ComponentModel.DataAnnotations;
namespace SmartAuditX.Models.ViewModels
{
    public class RegisterCompanyViewModel
    {
        [Required]
        [MaxLength(255)]
        public string CompanyName { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? IndustryType { get; set; }

        [MaxLength(500)]
        public string? LogoUrl { get; set; }
    }
}
