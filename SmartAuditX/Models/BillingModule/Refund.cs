// ─── Refund.cs ────────────────────────────────────────────────────────────

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SmartAuditX.Models.BillingModule.Enums;

namespace SmartAuditX.Models.BillingModule
{
    /// <summary>
    /// Dedicated refund ledger — separate from Payment table.
    /// One payment can have multiple partial refunds over time.
    /// Payment.RefundedAmount stores the aggregate; this table stores full history.
    /// </summary>
    [Table("Refunds")]
    public class Refund : AuditableEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int RefundId { get; set; }

        [Required]
        [ForeignKey("Payment")]
        public int PaymentId { get; set; }

        /// <summary>Gateway's own refund transaction ID for gateway-side lookup.</summary>
        [MaxLength(200)]
        public string? GatewayRefundId { get; set; }

        [Required]
        [Column(TypeName = "decimal(19,4)")]
        public decimal Amount { get; set; }

        [Required]
        [MaxLength(3)]
        public string Currency { get; set; } = "USD";

        [Column(TypeName = "nvarchar(20)")]
        public RefundReason Reason { get; set; } = RefundReason.CustomerRequest;

        [Column(TypeName = "nvarchar(10)")]
        public RefundStatus Status { get; set; } = RefundStatus.Pending;

        [MaxLength(500)]
        public string? FailureReason { get; set; }

        /// <summary>Which admin triggered this refund.</summary>
        public int? RequestedByUserId { get; set; }

        public DateTime? RefundedAt { get; set; }

        // ── Navigation ────────────────────────────────────────────────────
        public virtual Payment? Payment { get; set; }
    }
}