using Microsoft.AspNetCore.Http;
using SmartAuditX.Models;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using SmartAuditX.Extensions;
namespace SmartAuditX.Models.ViewModels
{
    public class RegisterCompanyViewModel
    {
        // ─────────────────────────────────────────────
        // COMPANY INFORMATION
        // ─────────────────────────────────────────────

        [Required]
        [StringLength(255)]
        public string CompanyName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please select your industry")]
        public IndustryType? IndustryType { get; set; }

        [Url]
        [StringLength(255)]
        public string? Website { get; set; }

        // ─────────────────────────────────────────────
        // COMPANY SIZE
        // ─────────────────────────────────────────────

        [Required]
        public CompanySize CompanySize { get; set; }

        [Required(ErrorMessage = "Please select employee count range")]
        public EmployeeCountRange? EmployeeCountRange { get; set; }

        // ─────────────────────────────────────────────
        // LOCATION
        // ─────────────────────────────────────────────

        // COUNTRY CODE (selected value)
        [Required(ErrorMessage = "Country is required")]
        [RegularExpression(@"^[A-Z]{2}$", ErrorMessage = "Invalid country selection")]
        public string CountryCode { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string City { get; set; } = string.Empty;

        // ─────────────────────────────────────────────
        // MARKETING
        // ─────────────────────────────────────────────

        [StringLength(100)]
        public string? ReferralSource { get; set; }

        // ─────────────────────────────────────────────
        // COMPANY LOGO
        // ─────────────────────────────────────────────

        public IFormFile? CompanyLogo { get; set; }

        // UI DROPDOWN SOURCE (NOT STORED IN DB)
        public List<SelectListItem>? Countries { get; set; }

        // UI Helper - Get industry list for dropdown
        public List<SelectListItem> IndustryList => Enum.GetValues(typeof(IndustryType))
            .Cast<IndustryType>()
            .Select(i => new SelectListItem
            {
                Value = i.ToString(),
                Text = i.GetDisplayName()
            }).ToList();

        // UI Helper - Get employee range list for dropdown
        public List<SelectListItem> EmployeeRangeList => Enum.GetValues(typeof(EmployeeCountRange))
            .Cast<EmployeeCountRange>()
            .Select(r => new SelectListItem
            {
                Value = r.ToString(),
                Text = r.GetDisplayName()
            }).ToList();
    }
}