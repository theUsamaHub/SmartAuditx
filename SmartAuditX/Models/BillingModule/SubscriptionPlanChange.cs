// ─── SubscriptionPlanChange.cs ────────────────────────────────────────────

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SmartAuditX.Models.BillingModule.Enums;

namespace SmartAuditX.Models.BillingModule
{
    /// <summary>
    /// Audit trail for every plan upgrade, downgrade, or renewal.
    /// Without this table you cannot reconstruct what plan a company was on,
    /// when they changed, or how much prorated credit was given —
    /// making billing disputes impossible to resolve.
    /// </summary>
    [Table("SubscriptionPlanChanges")]
    public class SubscriptionPlanChange : AuditableEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int SubscriptionPlanChangeId { get; set; }

        [Required]
        [ForeignKey("CompanySubscription")]
        public int CompanySubscriptionId { get; set; }

        [Required]
        [ForeignKey("FromPricing")]
        public int FromPricingId { get; set; }

        [Required]
        [ForeignKey("ToPricing")]
        public int ToPricingId { get; set; }

        [Column(TypeName = "nvarchar(15)")]
        public PlanChangeType ChangeType { get; set; }

        [Required]
        public DateTime EffectiveDate { get; set; }

        /// <summary>
        /// Credit given for unused days on the previous plan.
        /// Applied to next invoice automatically.
        /// </summary>
        [Column(TypeName = "decimal(19,4)")]
        public decimal ProratedCredit { get; set; } = 0;

        public int? ChangedByUserId { get; set; }

        // ── Navigation ────────────────────────────────────────────────────
        public virtual CompanySubscription? CompanySubscription { get; set; }

        //[ForeignKey("FromPricingId")]
        //public virtual SubscriptionPlanPricing? FromPricing { get; set; }

        //[ForeignKey("ToPricingId")]
        //public virtual SubscriptionPlanPricing? ToPricing { get; set; }
    }
}