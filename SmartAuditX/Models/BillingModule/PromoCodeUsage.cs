// ─── PromoCodeUsage.cs ────────────────────────────────────────────────────

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartAuditX.Models.BillingModule
{
    /// <summary>
    /// Tracks each individual use of a promo code.
    /// Enables per-company limit enforcement and usage analytics.
    /// </summary>
    [Table("PromoCodeUsages")]
    public class PromoCodeUsage :AuditableEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PromoCodeUsageId { get; set; }

        [Required]
        [ForeignKey("PromoCode")]
        public int PromoCodeId { get; set; }

        [Required]
        [ForeignKey("Company")]
        public int CompanyId { get; set; }

        [Required]
        [ForeignKey("Payment")]
        public int PaymentId { get; set; }

        /// <summary>Actual monetary discount applied to this payment.</summary>
        [Required]
        [Column(TypeName = "decimal(19,4)")]
        public decimal DiscountApplied { get; set; }

        public DateTime UsedAt { get; set; } = DateTime.UtcNow;

        // ── Navigation ────────────────────────────────────────────────────
        public virtual PromoCode? PromoCode { get; set; }
        public virtual Company? Company { get; set; }
        public virtual Payment? Payment { get; set; }
    }
}