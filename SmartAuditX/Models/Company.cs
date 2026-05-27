using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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
    /// Broad classification of company size.
    /// Used for analytics, segmentation, and plan recommendation.
    /// The actual enforced employee limit comes from SubscriptionPlanFeature (MaxEmployees),
    /// not from this field.
    /// </summary>
    public enum CompanySize
    {
        Small,      // Informational: roughly 1–50 employees
        Medium,     // Informational: roughly 51–200 employees
        Large,      // Informational: roughly 201–1000 employees
        Enterprise  // Informational: 1000+ employees
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
        /// Broad industry category (e.g. Retail, Healthcare, Education, Manufacturing).
        /// Free text — not an enum — so new industries don't require a migration.
        /// </summary>
        [MaxLength(100)]
        public string? IndustryType { get; set; }

        /// <summary>URL to the company logo stored in your file storage (S3, Azure Blob, etc.).</summary>
        [MaxLength(500)]
        public string? LogoUrl { get; set; }

        /// <summary>Company website. Optional but useful for admin panel and targeting.</summary>
        [MaxLength(255)]
        [Url(ErrorMessage = "Website must be a valid URL")]
        public string? Website { get; set; }

        /// <summary>
        /// Legal registration number (SECP number for Pakistan, Companies House for UK, etc.).
        /// Optional — not all SMBs have formal registration.
        /// </summary>
        //[MaxLength(100)]
        //public string? RegistrationNumber { get; set; }

        /// <summary>
        /// Tax identification number.
        /// NTN for Pakistan, GST/VAT number for other regions.
        /// Required for invoice generation in some countries.
        /// </summary>
        //[MaxLength(100)]
        //public string? TaxNumber { get; set; }


        // ── Size & Segmentation ───────────────────────────────────────────────

        /// <summary>
        /// Broad company size band — INFORMATIONAL ONLY.
        /// This is what the company tells you at signup for analytics/targeting.
        /// The enforced employee limit comes from SubscriptionPlanFeature (MaxEmployees).
        /// </summary>
        [Column(TypeName = "nvarchar(20)")]
        public CompanySize? CompanySize { get; set; }

        /// <summary>
        /// Self-reported employee count range selected during onboarding.
        /// Used to recommend the right plan on the Plan Selection screen.
        /// e.g. "1-50", "51-200", "201-1000", "1000+"
        /// </summary>
        [MaxLength(20)]
        public string? EmployeeCountRange { get; set; }


        // ── Location ──────────────────────────────────────────────────────────

        /// <summary>
        /// ISO 3166-1 alpha-2 country code. e.g. "PK", "US", "AE", "GB".
        /// No Countries table needed — frontend loads from a static JSON list.
        /// Stored as 2-letter code to keep it compact and standard.
        /// </summary>
        [MaxLength(2, ErrorMessage = "Country code must be a 2-letter ISO code")]
        [MinLength(2, ErrorMessage = "Country code must be a 2-letter ISO code")]
        public string? CountryCode { get; set; }

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
        [MaxLength(100)]
        public string? ReferralSource { get; set; }

        /// <summary>
        /// Tracks which step of the onboarding funnel the company is at.
        /// Used to redirect the admin to the correct step when they return.
        /// CompanyInfoSaved → EmailVerified → Active.
        /// Does NOT roll back on payment failure — that is handled by the Payment module.
        /// </summary>
        [Required]
        [Column(TypeName = "nvarchar(30)")]
        public OnboardingStatus OnboardingStatus { get; set; } = OnboardingStatus.CompanyInfoSaved;

        /// <summary>
        /// Stores the plan the company selected on the Plan Selection screen
        /// but has not yet paid for. Nullable — null means no plan selected yet.
        /// Allows resuming the payment page with the plan pre-selected.
        /// Cleared (set to null) after successful payment.
        /// The actual active plan is tracked in CompanySubscription after payment.
        /// </summary>
        //[ForeignKey("SelectedPlanPricing")]
        //public int? SelectedPlanPricingId { get; set; }


        // ── Soft Delete & Timestamps ──────────────────────────────────────────

        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }


        // ── Navigation Properties ─────────────────────────────────────────────

        //public virtual SubscriptionPlanPricing? SelectedPlanPricing { get; set; }

        public virtual ICollection<Branch> Branches { get; set; } = new List<Branch>();
        public virtual ICollection<Department> Departments { get; set; } = new List<Department>();
        public virtual ICollection<CompanyContact> CompanyContacts { get; set; } = new List<CompanyContact>();
        public virtual ICollection<Designation> Designations { get; set; } = new List<Designation>();
        public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();
        public virtual ICollection<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();
        public virtual ICollection<CompanySubscription> Subscriptions { get; set; } = new List<CompanySubscription>();
    }
}