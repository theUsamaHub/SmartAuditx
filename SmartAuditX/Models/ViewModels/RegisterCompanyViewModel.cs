using Microsoft.AspNetCore.Http;
using SmartAuditX.Models;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using SmartAuditX.Extensions;
namespace SmartAuditX.Models.ViewModels
{

    //// ─── ENUMS ────────────────────────────────────────────────────────────────

    ///// <summary>
    ///// Tracks where the company is in the onboarding funnel.
    ///// Only 3 states — moves forward only, never rolls back.
    ///// Payment failures are handled by the Payment module, not here.
    ///// </summary>
    //public enum OnboardingStatus
    //{
    //    /// <summary>
    //    /// Basic form (phone/email/username) + Company info saved together
    //    /// in one atomic insert. This is the starting state.
    //    /// </summary>
    //    CompanyInfoSaved,

    //    /// <summary>
    //    /// Admin user confirmed their email via OTP or verification link.
    //    /// System redirects to Plan Selection at this point.
    //    /// </summary>
    //    EmailVerified,

    //    /// <summary>
    //    /// Payment was successful. Company has full access to the platform.
    //    /// This is the only state where the company can use the system.
    //    /// </summary>
    //    Active
    //}

    ///// <summary>
    ///// Industry classification based on UN ISIC Rev.4 standard
    ///// Add new industries here as needed - no database migration required
    ///// </summary>
    //public enum IndustryType
    //{
    //    [Display(Name = "Agriculture, Forestry & Fishing")]
    //    Agriculture,

    //    [Display(Name = "Mining & Quarrying")]
    //    Mining,

    //    [Display(Name = "Manufacturing")]
    //    Manufacturing,

    //    [Display(Name = "Electricity, Gas & Water Supply")]
    //    Utilities,

    //    [Display(Name = "Construction")]
    //    Construction,

    //    [Display(Name = "Wholesale & Retail Trade")]
    //    RetailTrade,

    //    [Display(Name = "Transportation & Storage")]
    //    Transportation,

    //    [Display(Name = "Accommodation & Food Services")]
    //    Hospitality,

    //    [Display(Name = "Information & Communication")]
    //    IT,

    //    [Display(Name = "Financial & Insurance Activities")]
    //    Finance,

    //    [Display(Name = "Real Estate Activities")]
    //    RealEstate,

    //    [Display(Name = "Professional, Scientific & Technical")]
    //    ProfessionalServices,

    //    [Display(Name = "Administrative & Support Services")]
    //    Administrative,

    //    [Display(Name = "Public Administration & Defense")]
    //    PublicSector,

    //    [Display(Name = "Education")]
    //    Education,

    //    [Display(Name = "Human Health & Social Work")]
    //    Healthcare,

    //    [Display(Name = "Arts, Entertainment & Recreation")]
    //    Arts,

    //    [Display(Name = "Other Service Activities")]
    //    OtherServices,

    //    [Display(Name = "Non-Profit Organization")]
    //    NonProfit,

    //    [Display(Name = "Startup / Early Stage")]
    //    Startup,

    //    [Display(Name = "E-commerce / Online Retail")]
    //    Ecommerce,

    //    [Display(Name = "Consulting")]
    //    Consulting,

    //    [Display(Name = "Legal Services")]
    //    Legal,

    //    [Display(Name = "Accounting & Tax")]
    //    Accounting,

    //    [Display(Name = "Marketing & Advertising")]
    //    Marketing,

    //    [Display(Name = "Real Estate Development")]
    //    RealEstateDevelopment,

    //    [Display(Name = "Logistics & Supply Chain")]
    //    Logistics,

    //    [Display(Name = "Telecommunications")]
    //    Telecom,

    //    [Display(Name = "Media & Publishing")]
    //    Media,

    //    [Display(Name = "Gaming")]
    //    Gaming,

    //    [Display(Name = "Biotechnology")]
    //    Biotech,

    //    [Display(Name = "Pharmaceuticals")]
    //    Pharma,

    //    [Display(Name = "Energy (Oil, Gas, Solar, Wind)")]
    //    Energy,

    //    [Display(Name = "Automotive")]
    //    Automotive,

    //    [Display(Name = "Aerospace & Defense")]
    //    Aerospace,

    //    [Display(Name = "Textiles & Apparel")]
    //    Textiles,

    //    [Display(Name = "Food & Beverage")]
    //    FoodBeverage,

    //    [Display(Name = "Chemicals")]
    //    Chemicals,

    //    [Display(Name = "Plastics & Rubber")]
    //    Plastics,

    //    [Display(Name = "Metals & Mining")]
    //    Metals,

    //    [Display(Name = "Paper & Packaging")]
    //    Paper,

    //    [Display(Name = "Printing & Publishing")]
    //    Printing,

    //    [Display(Name = "Furniture & Fixtures")]
    //    Furniture,

    //    [Display(Name = "Machinery & Equipment")]
    //    Machinery,

    //    [Display(Name = "Electronics")]
    //    Electronics,

    //    [Display(Name = "Medical Devices")]
    //    MedicalDevices,

    //    [Display(Name = "Sports & Recreation")]
    //    Sports,

    //    [Display(Name = "Travel & Tourism")]
    //    Travel,

    //    [Display(Name = "Event Management")]
    //    Events,

    //    [Display(Name = "Photography")]
    //    Photography,

    //    [Display(Name = "Beauty & Wellness")]
    //    Beauty,

    //    [Display(Name = "Fitness & Gyms")]
    //    Fitness,

    //    [Display(Name = "Pet Services")]
    //    PetServices,

    //    [Display(Name = "Cleaning Services")]
    //    Cleaning,

    //    [Display(Name = "Security Services")]
    //    Security,
    //}

    ///// <summary>
    ///// Employee count ranges for company size segmentation
    ///// Add new ranges here as needed - no database migration required
    ///// </summary>
    //public enum EmployeeCountRange
    //{
    //    [Display(Name = "Just me (1)")]
    //    JustMe,

    //    [Display(Name = "2-10 employees")]
    //    VerySmall,

    //    [Display(Name = "11-50 employees")]
    //    Small,

    //    [Display(Name = "51-200 employees")]
    //    Medium,

    //    [Display(Name = "201-500 employees")]
    //    Large,

    //    [Display(Name = "501-1000 employees")]
    //    VeryLarge,

    //    [Display(Name = "1001-5000 employees")]
    //    Enterprise,

    //    [Display(Name = "5000+ employees")]
    //    MegaCorp,
    //}

    ///// <summary>
    ///// Employee count ranges for company size segmentation
    ///// Add new ranges here as needed - no database migration required
    ///// </summary>

    //public enum ReferralSources
    //{
    //    Youtube,
    //    Instagram,
    //    Facebook,

    //}
    ///// <summary>
    ///// Broad classification of company size.
    ///// Used for analytics, segmentation, and plan recommendation.
    ///// The actual enforced employee limit comes from SubscriptionPlanFeature (MaxEmployees),
    ///// not from this field.
    ///// </summary>
    //public enum CompanySize
    //{
    //    [Display(Name = "Small (1-50 employees)")]
    //    Small,

    //    [Display(Name = "Medium (51-200 employees)")]
    //    Medium,

    //    [Display(Name = "Large (201-1000 employees)")]
    //    Large,

    //    [Display(Name = "Enterprise (1000+ employees)")]
    //    Enterprise
    //}
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
        [MaxLength(100)]
        public string City { get; set; } = string.Empty;

        // ─────────────────────────────────────────────
        // MARKETING
        // ─────────────────────────────────────────────

        public ReferralSources? ReferralSource { get; set; }

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
        public List<SelectListItem> ReferralSourceList => Enum.GetValues(typeof(ReferralSources))
          .Cast<ReferralSources>()
          .Select(r => new SelectListItem
          {
              Value = r.ToString(),
              Text = r.GetDisplayName()
          }).ToList();
    }
}