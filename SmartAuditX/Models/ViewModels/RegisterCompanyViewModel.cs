using System.ComponentModel.DataAnnotations;
namespace SmartAuditX.Models.ViewModels
{
    public class RegisterCompanyViewModel
    {

        [Required]
        public string CompanyName { get; set; }

        public string? IndustryType { get; set; }

        public string? LogoUrl { get; set; }
    }
}
