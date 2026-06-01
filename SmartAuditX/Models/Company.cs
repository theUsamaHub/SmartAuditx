using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

namespace SmartAuditX.Models
{
    // ─── ENUMS ────────────────────────────────────────────────────────────────

    /// <summary>
    /// Tracks where the company is in the onboarding funnel.
    /// Only 3 states — moves forward only, never rolls back.
    /// Payment failures are handled by the Payment module, not here.
    /// </summary>
    public enum OnboardingStatus
    {
        /// <summary>
        /// Basic form (phone/email/username) + Company info saved together
        /// in one atomic insert. This is the starting state.
        /// </summary>
        CompanyInfoSaved,

        /// <summary>
        /// Admin user confirmed their email via OTP or verification link.
        /// System redirects to Plan Selection at this point.
        /// </summary>
        EmailVerified,

        /// <summary>
        /// Payment was successful. Company has full access to the platform.
        /// This is the only state where the company can use the system.
        /// </summary>
        Active
    }

    /// <summary>
    /// Industry classification based on UN ISIC Rev.4 standard
    /// Add new industries here as needed - no database migration required
    /// </summary>
    public enum IndustryType
    {
        [Display(Name = "Agriculture, Forestry & Fishing")]
        Agriculture,

        [Display(Name = "Mining & Quarrying")]
        Mining,

        [Display(Name = "Manufacturing")]
        Manufacturing,

        [Display(Name = "Electricity, Gas & Water Supply")]
        Utilities,

        [Display(Name = "Construction")]
        Construction,

        [Display(Name = "Wholesale & Retail Trade")]
        RetailTrade,

        [Display(Name = "Transportation & Storage")]
        Transportation,

        [Display(Name = "Accommodation & Food Services")]
        Hospitality,

        [Display(Name = "Information & Communication")]
        IT,

        [Display(Name = "Financial & Insurance Activities")]
        Finance,

        [Display(Name = "Real Estate Activities")]
        RealEstate,

        [Display(Name = "Professional, Scientific & Technical")]
        ProfessionalServices,

        [Display(Name = "Administrative & Support Services")]
        Administrative,

        [Display(Name = "Public Administration & Defense")]
        PublicSector,

        [Display(Name = "Education")]
        Education,

        [Display(Name = "Human Health & Social Work")]
        Healthcare,

        [Display(Name = "Arts, Entertainment & Recreation")]
        Arts,

        [Display(Name = "Other Service Activities")]
        OtherServices,

        [Display(Name = "Non-Profit Organization")]
        NonProfit,

        [Display(Name = "Startup / Early Stage")]
        Startup,

        [Display(Name = "E-commerce / Online Retail")]
        Ecommerce,

        [Display(Name = "Consulting")]
        Consulting,

        [Display(Name = "Legal Services")]
        Legal,

        [Display(Name = "Accounting & Tax")]
        Accounting,

        [Display(Name = "Marketing & Advertising")]
        Marketing,

        [Display(Name = "Real Estate Development")]
        RealEstateDevelopment,

        [Display(Name = "Logistics & Supply Chain")]
        Logistics,

        [Display(Name = "Telecommunications")]
        Telecom,

        [Display(Name = "Media & Publishing")]
        Media,

        [Display(Name = "Gaming")]
        Gaming,

        [Display(Name = "Biotechnology")]
        Biotech,

        [Display(Name = "Pharmaceuticals")]
        Pharma,

        [Display(Name = "Energy (Oil, Gas, Solar, Wind)")]
        Energy,

        [Display(Name = "Automotive")]
        Automotive,

        [Display(Name = "Aerospace & Defense")]
        Aerospace,

        [Display(Name = "Textiles & Apparel")]
        Textiles,

        [Display(Name = "Food & Beverage")]
        FoodBeverage,

        [Display(Name = "Chemicals")]
        Chemicals,

        [Display(Name = "Plastics & Rubber")]
        Plastics,

        [Display(Name = "Metals & Mining")]
        Metals,

        [Display(Name = "Paper & Packaging")]
        Paper,

        [Display(Name = "Printing & Publishing")]
        Printing,

        [Display(Name = "Furniture & Fixtures")]
        Furniture,

        [Display(Name = "Machinery & Equipment")]
        Machinery,

        [Display(Name = "Electronics")]
        Electronics,

        [Display(Name = "Medical Devices")]
        MedicalDevices,

        [Display(Name = "Sports & Recreation")]
        Sports,

        [Display(Name = "Travel & Tourism")]
        Travel,

        [Display(Name = "Event Management")]
        Events,

        [Display(Name = "Photography")]
        Photography,

        [Display(Name = "Beauty & Wellness")]
        Beauty,

        [Display(Name = "Fitness & Gyms")]
        Fitness,

        [Display(Name = "Pet Services")]
        PetServices,

        [Display(Name = "Cleaning Services")]
        Cleaning,

        [Display(Name = "Security Services")]
        Security,
    }

    /// <summary>
    /// Employee count ranges for company size segmentation
    /// Add new ranges here as needed - no database migration required
    /// </summary>
    public enum EmployeeCountRange
    {
        [Display(Name = "Just me (1)")]
        JustMe,

        [Display(Name = "2-10 employees")]
        VerySmall,

        [Display(Name = "11-50 employees")]
        Small,

        [Display(Name = "51-200 employees")]
        Medium,

        [Display(Name = "201-500 employees")]
        Large,

        [Display(Name = "501-1000 employees")]
        VeryLarge,

        [Display(Name = "1001-5000 employees")]
        Enterprise,

        [Display(Name = "5000+ employees")]
        MegaCorp,
    }

    /// <summary>
    /// Employee count ranges for company size segmentation
    /// Add new ranges here as needed - no database migration required
    /// </summary>
    
    public enum ReferralSources
    {
        Youtube,
        Instagram,
        Facebook,

    }
    /// <summary>
    /// Broad classification of company size.
    /// Used for analytics, segmentation, and plan recommendation.
    /// The actual enforced employee limit comes from SubscriptionPlanFeature (MaxEmployees),
    /// not from this field.
    /// </summary>
    public enum CompanySize
    {
        [Display(Name = "Small (1-50 employees)")]
        Small,

        [Display(Name = "Medium (51-200 employees)")]
        Medium,

        [Display(Name = "Large (201-1000 employees)")]
        Large,

        [Display(Name = "Enterprise (1000+ employees)")]
        Enterprise
    }

    // ─── COMPANY MODEL ────────────────────────────────────────────────────────

    /// <summary>
    /// Core tenant entity. Every record in the system belongs to a Company.
    /// Created during onboarding Step 1 (Basic Form + Company Info) in a single
    /// atomic transaction alongside the ApplicationUser (admin) record.
    /// </summary>
    [Table("Companies")]
    public class Company
    {
        // ── Primary Key ───────────────────────────────────────────────────────

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CompanyId { get; set; }

        // ── Core Identity ─────────────────────────────────────────────────────

        /// <summary>Legal or trading name of the company.</summary>
        [Required(ErrorMessage = "Company name is required")]
        [MaxLength(255)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Industry classification using IndustryType enum
        /// </summary>
        [Column(TypeName = "nvarchar(50)")]
        public IndustryType? IndustryType { get; set; }

        /// <summary>URL to the company logo stored in your file storage (S3, Azure Blob, etc.).</summary>
        [MaxLength(500)]
        public string? LogoUrl { get; set; }

        /// <summary>Company website. Optional but useful for admin panel and targeting.</summary>
        [MaxLength(255)]
        [Url(ErrorMessage = "Website must be a valid URL")]
        public string? Website { get; set; }

        // ── Size & Segmentation ───────────────────────────────────────────────

        /// <summary>
        /// Broad company size band — INFORMATIONAL ONLY.
        /// This is what the company tells you at signup for analytics/targeting.
        /// The enforced employee limit comes from SubscriptionPlanFeature (MaxEmployees).
        /// </summary>
        [Column(TypeName = "nvarchar(20)")]
        public CompanySize? CompanySize { get; set; }

        /// <summary>
        /// Detailed employee count range using EmployeeCountRange enum
        /// </summary>
        [Column(TypeName = "nvarchar(30)")]
        public EmployeeCountRange? EmployeeCountRange { get; set; }

        // ── Location ──────────────────────────────────────────────────────────

        /// <summary>
        /// ISO 3166-1 alpha-2 country code. e.g. "PK", "US", "AE", "GB".
        /// No Countries table needed — frontend loads from a static JSON list.
        /// Stored as 2-letter code to keep it compact and standard.
        /// </summary>
        [RegularExpression(@"^[A-Z]{2}$", ErrorMessage = "Invalid country selection.")]
        [Required]
        public string CountryCode { get; set; }

        /// <summary>
        /// Free-text city name. No Cities table — too large to maintain (150k+ rows).
        /// Standard approach used by Stripe, HubSpot, Slack.
        /// </summary>
        [MaxLength(100)]
        public string? City { get; set; }

        // ── Onboarding & Marketing ────────────────────────────────────────────

        /// <summary>
        /// How the company found OpsPulse.
        /// e.g. "Google", "LinkedIn", "Referral", "Event", "Cold Outreach".
        /// Free text — not enum — so new sources don't require a migration.
        /// Used for marketing attribution and ROI tracking.
        /// </summary>

        public ReferralSources? ReferralSource { get; set; }

        /// <summary>
        /// Tracks which step of the onboarding funnel the company is at.
        /// Used to redirect the admin to the correct step when they return.
        /// CompanyInfoSaved → EmailVerified → Active.
        /// Does NOT roll back on payment failure — that is handled by the Payment module.
        /// </summary>
        [Required]
        [Column(TypeName = "nvarchar(30)")]
        public OnboardingStatus OnboardingStatus { get; set; } = OnboardingStatus.CompanyInfoSaved;



        // ── Soft Delete & Timestamps ──────────────────────────────────────────

        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // ── Navigation Properties ─────────────────────────────────────────────

        public virtual ICollection<Branch> Branches { get; set; } = new List<Branch>();
        public virtual ICollection<Department> Departments { get; set; } = new List<Department>();
        public virtual ICollection<CompanyContact> CompanyContacts { get; set; } = new List<CompanyContact>();
        public virtual ICollection<Designation> Designations { get; set; } = new List<Designation>();
        public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();
        public virtual ICollection<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();
        public virtual ICollection<CompanySubscription> Subscriptions { get; set; } = new List<CompanySubscription>();
    }
}