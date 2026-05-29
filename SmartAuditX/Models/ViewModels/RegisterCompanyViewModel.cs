using Microsoft.AspNetCore.Http;
using SmartAuditX.Models;
using System.ComponentModel.DataAnnotations;

using Microsoft.AspNetCore.Mvc.Rendering;

namespace SmartAuditX.Models.ViewModels
{

    public enum IndustryType
    {
        Hellom,
        gggg,
        usama,

    }

    public class RegisterCompanyViewModel
    {
        // ─────────────────────────────────────────────
        // COMPANY INFORMATION
        // ─────────────────────────────────────────────

        [Required]
        [StringLength(255)]
        public string CompanyName { get; set; } = string.Empty;

        public string? IndustryType { get; set; }

        [Url]
        [StringLength(255)]
        public string? Website { get; set; }

        // ─────────────────────────────────────────────
        // COMPANY SIZE
        // ─────────────────────────────────────────────

        [Required]
        public CompanySize CompanySize { get; set; }

        [Required]
        [StringLength(20)]
        public string EmployeeCountRange { get; set; } = string.Empty;

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
    }

}
