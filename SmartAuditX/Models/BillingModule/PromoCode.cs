// ─── PromoCode.cs ─────────────────────────────────────────────────────────

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SmartAuditX.Models.BillingModule.Enums;

namespace SmartAuditX.Models.BillingModule
{
    /// <summary>
    /// Discount codes for growth: launch offers, referral rewards, sales.
    /// MaxUsageCount limits global uses. PerCompanyLimit (usually 1)
    /// prevents the same company using a code multiple times.
    /// ApplicablePlanId null = applies to all plans.
    /// </summary>
    [Table("PromoCodes")]
    public class PromoCode : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PromoCodeId { get; set; }

        /// <summary>e.g. "LAUNCH50", "APTECH2025".</summary>
        [Required]
        [MaxLength(50)]
        public string Code { get; set; } = string.Empty;

        [Column(TypeName = "nvarchar(15)")]
        public DiscountType DiscountType { get; set; } = DiscountType.Percentage;

        /// <summary>50 for 50% off, or 500.0000 for flat PKR 500.</summary>
        [Required]
        [Column(TypeName = "decimal(19,4)")]
        public decimal DiscountValue { get; set; }

        /// <summary>Null = unlimited uses.</summary>
        public int? MaxUsageCount { get; set; }

        public int UsedCount { get; set; } = 0;

        public int PerCompanyLimit { get; set; } = 1;

        [ForeignKey("SubscriptionPlan")]
        public int? ApplicablePlanId { get; set; }

        public DateTime ValidFrom { get; set; }
        public DateTime ValidUntil { get; set; }

        [NotMapped]
        public bool IsValid =>
            IsActive &&
            DateTime.UtcNow >= ValidFrom &&
            DateTime.UtcNow <= ValidUntil &&
            (MaxUsageCount == null || UsedCount < MaxUsageCount);

        // ── Navigation ────────────────────────────────────────────────────
        public virtual SubscriptionPlan? SubscriptionPlan { get; set; }
        public virtual ICollection<PromoCodeUsage> Usages { get; set; }
            = new List<PromoCodeUsage>();
    }
}