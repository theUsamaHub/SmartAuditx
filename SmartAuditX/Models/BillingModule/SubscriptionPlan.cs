// ─── SubscriptionPlan.cs ──────────────────────────────────────────────────

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartAuditX.Models.BillingModule
{
    /// <summary>
    /// Top-level plan tier. e.g. Basic, Professional, Enterprise.
    /// No pricing stored here — pricing lives in SubscriptionPlanPricing
    /// to support multiple billing cycles and historical accuracy.
    /// </summary>
    [Table("SubscriptionPlans")]
    public class SubscriptionPlan : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int SubscriptionPlanId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        /// <summary>
        /// Free trial days offered for this plan.
        /// 0 = no trial.
        /// </summary>
        [Range(0, 90)]
        public int TrialDays { get; set; } = 0;

        // ── Navigation ────────────────────────────────────────────────────
        public virtual ICollection<SubscriptionPlanPricing> PricingOptions { get; set; }
            = new List<SubscriptionPlanPricing>();
        public virtual ICollection<SubscriptionPlanFeature> Features { get; set; }
            = new List<SubscriptionPlanFeature>();
    }
}