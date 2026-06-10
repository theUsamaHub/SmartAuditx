// ─── CompanyCredit.cs ─────────────────────────────────────────────────────

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SmartAuditX.Models.BillingModule.Enums;

namespace SmartAuditX.Models.BillingModule
{
    /// <summary>
    /// Company credit wallet — balance applied to future invoices.
    /// Faster than gateway refunds (instant vs 5-10 business days).
    /// Sources: plan change proration, goodwill, referral bonuses, promos.
    /// ExpiresAt prevents stale credit accumulation.
    /// </summary>
    [Table("CompanyCredits")]
    public class CompanyCredit : AuditableEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CompanyCreditId { get; set; }

        [Required]
        [ForeignKey("Company")]
        public int CompanyId { get; set; }

        [Required]
        [Column(TypeName = "decimal(19,4)")]
        public decimal Amount { get; set; }

        [Required]
        [MaxLength(3)]
        public string Currency { get; set; } = "USD";

        [Column(TypeName = "nvarchar(20)")]
        public CreditReason Reason { get; set; }

        public DateTime? ExpiresAt { get; set; }

        public bool IsUsed { get; set; } = false;

        [ForeignKey("UsedInPayment")]
        public int? UsedInPaymentId { get; set; }

        // ── Navigation ────────────────────────────────────────────────────
        public virtual Company? Company { get; set; }
        public virtual Payment? UsedInPayment { get; set; }
    }
}