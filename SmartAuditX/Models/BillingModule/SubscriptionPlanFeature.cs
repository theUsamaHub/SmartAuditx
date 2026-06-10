// ─── SubscriptionPlanFeature.cs ───────────────────────────────────────────

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartAuditX.Models.BillingModule
{
    /// <summary>
    /// Feature flags and limits per subscription plan.
    /// Enables dynamic UI rendering without hardcoding feature lists.
    /// FeatureName examples: MaxEmployees, MaxBranches, AuditModule, APIAccess.
    /// FeatureValue examples: "Enabled", "Disabled", "50", "Unlimited".
    /// </summary>
    [Table("SubscriptionPlanFeatures")]
    public class SubscriptionPlanFeature : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int SubscriptionPlanFeatureId { get; set; }

        [Required]
        [ForeignKey("SubscriptionPlan")]
        public int SubscriptionPlanId { get; set; }

        [Required]
        [MaxLength(100)]
        public string FeatureName { get; set; } = string.Empty;

        /// <summary>"Enabled", "Disabled", "10", "Unlimited".</summary>
        [Required]
        [MaxLength(255)]
        public string FeatureValue { get; set; } = string.Empty;

        [NotMapped]
        public bool IsEnabled =>
            FeatureValue.Equals("Enabled", StringComparison.OrdinalIgnoreCase);

        [NotMapped]
        public int? NumericLimit =>
            int.TryParse(FeatureValue, out int result) ? result : null;

        // ── Navigation ────────────────────────────────────────────────────
        public virtual SubscriptionPlan? SubscriptionPlan { get; set; }
    }
}