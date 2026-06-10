//// ─── SubscriptionPlanPricing.cs ───────────────────────────────────────────

//using System.ComponentModel.DataAnnotations;
//using System.ComponentModel.DataAnnotations.Schema;
//using SmartAuditX.Models.BillingModule.Enums;

//namespace SmartAuditX.Models.BillingModule
//{
//    /// <summary>
//    /// Pricing per plan per billing cycle.
//    /// Historical accuracy: price changes deactivate old rows and create new ones.
//    /// CompanySubscriptions always reference a specific pricing row.
//    /// decimal(19,4) handles PKR scale and FX/tax precision.
//    /// </summary>
//    [Table("SubscriptionPlanPricing")]
//    public class SubscriptionPlanPricing : AuditableEntity
//    {
//        [Key]
//        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
//        public int SubscriptionPlanPricingId { get; set; }

//        [Required]
//        [ForeignKey("SubscriptionPlan")]
//        public int SubscriptionPlanId { get; set; }

//        [Required]
//        [Column(TypeName = "nvarchar(20)")]
//        public BillingCycle BillingCycle { get; set; }


//        public int DurationInMonths  {get;set; }
//        [Required]
//        [Column(TypeName = "decimal(19,4)")]
//        public decimal Price { get; set; }

//        /// <summary>
//        /// ISO 4217 currency code.
//        /// Validated in service layer — not hardcoded regex.
//        /// e.g. "PKR", "USD", "AED".
//        /// </summary>
//        [Required]
//        [MaxLength(3)]
//        public string Currency { get; set; } = "USD";

//        [Range(0, 100)]
//        [Column(TypeName = "decimal(5,2)")]
//        public decimal DiscountPercentage { get; set; } = 0;

//        [NotMapped]
//        public decimal FinalPrice => Price - (Price * (DiscountPercentage / 100));

//        // ── Navigation ────────────────────────────────────────────────────
//        public virtual SubscriptionPlan? SubscriptionPlan { get; set; }
//        public virtual ICollection<CompanySubscription> CompanySubscriptions { get; set; }
//            = new List<CompanySubscription>();
//    }
//}
// ─── SubscriptionPlanPricing.cs ───────────────────────────────────────────

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SmartAuditX.Models.BillingModule.Enums;

namespace SmartAuditX.Models.BillingModule
{
    /// <summary>
    /// Represents a specific purchasable pricing option for a subscription plan.
    ///
    /// Examples:
    /// Basic Plan
    /// ├─ Monthly   → 1 Month  → PKR 2,000
    /// ├─ Quarterly → 3 Months → PKR 5,500
    /// ├─ Yearly    → 12 Months → PKR 20,000
    ///
    /// Historical pricing is preserved by creating a new pricing row
    /// whenever pricing changes occur instead of updating existing rows.
    ///
    /// Company subscriptions reference a specific pricing record to ensure
    /// invoices, payments, and renewals always remain historically accurate.
    ///
    /// Monetary values use decimal(19,4) to support:
    /// - Large financial amounts
    /// - Tax calculations
    /// - Multi-currency support
    /// - Future gateway integrations
    /// </summary>
    [Table("SubscriptionPlanPricing")]
    public class SubscriptionPlanPricing : AuditableEntity
    {
        /// <summary>
        /// Primary key.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int SubscriptionPlanPricingId { get; set; }

        /// <summary>
        /// Parent subscription plan.
        /// Example:
        /// Basic
        /// Professional
        /// Enterprise
        /// </summary>
        [Required]
        [ForeignKey(nameof(SubscriptionPlan))]
        public int SubscriptionPlanId { get; set; }

        /// <summary>
        /// Billing cycle type selected for this pricing option.
        ///
        /// Examples:
        /// Monthly
        /// Quarterly
        /// BiAnnual
        /// Yearly
        ///
        /// Used mainly for UI display and reporting.
        /// Actual subscription duration calculations should use
        /// DurationInMonths to avoid hardcoded business logic.
        /// </summary>
        [Required]
        [Column(TypeName = "nvarchar(20)")]
        public BillingCycle BillingCycle { get; set; }

        /// <summary>
        /// Exact duration covered by this pricing option.
        ///
        /// Examples:
        /// Monthly   = 1
        /// Quarterly = 3
        /// BiAnnual  = 6
        /// Yearly    = 12
        ///
        /// Renewal calculations, expiry calculations,
        /// upgrades, downgrades, and invoice generation
        /// should always use this value.
        ///
        /// This avoids tightly coupling billing logic
        /// to enum values.
        /// </summary>
        [Required]
        [Range(1, 120)]
        public int DurationInMonths { get; set; }

        /// <summary>
        /// Base plan price before any discounts.
        ///
        /// Examples:
        /// 2000.00
        /// 5500.00
        /// 20000.00
        /// </summary>
        [Required]
        [Column(TypeName = "decimal(19,4)")]
        public decimal Price { get; set; }

        /// <summary>
        /// ISO 4217 currency code.
        ///
        /// Examples:
        /// PKR
        /// USD
        /// AED
        /// SAR
        ///
        /// Validation should be performed in the service layer.
        /// </summary>
        [Required]
        [MaxLength(3)]
        public string Currency { get; set; } = "USD";

        /// <summary>
        /// Optional percentage discount applied to this pricing option.
        ///
        /// Examples:
        /// 0   = No Discount
        /// 10  = 10% Discount
        /// 25  = 25% Discount
        ///
        /// Useful for:
        /// - Launch promotions
        /// - Annual plan savings
        /// - Seasonal offers
        /// </summary>
        [Range(0, 100)]
        [Column(TypeName = "decimal(5,2)")]
        public decimal DiscountPercentage { get; set; } = 0;

        /// <summary>
        /// Final customer-facing amount after discount.
        ///
        /// Example:
        /// Price = 1000
        /// DiscountPercentage = 20
        ///
        /// FinalPrice = 800
        /// </summary>
        [NotMapped]
        public decimal FinalPrice =>
            Price - (Price * (DiscountPercentage / 100));

        // ─────────────────────────────────────────────
        // Navigation Properties
        // ─────────────────────────────────────────────

        /// <summary>
        /// Parent subscription plan.
        /// </summary>
        public virtual SubscriptionPlan? SubscriptionPlan { get; set; }

        /// <summary>
        /// Company subscriptions purchased using this pricing option.
        /// </summary>
        public virtual ICollection<CompanySubscription> CompanySubscriptions { get; set; }
            = new List<CompanySubscription>();
    }
}