// ─── Payment.cs ───────────────────────────────────────────────────────────

using Microsoft.CodeAnalysis;
using SmartAuditX.Models.BillingModule.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartAuditX.Models.BillingModule
{
    /// <summary>
    /// Immutable payment ledger. Append-only — never updated, never deleted.
    /// One row = one payment intent. Multiple attempts live in PaymentAttempts.
    /// CRITICAL: Full card numbers are NEVER stored — PCI-DSS violation.
    /// Only last4, brand, expiry, and gateway token are stored.
    /// </summary>
    [Table("Payments")]
    public class Payment : AuditableEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PaymentId { get; set; }

        [Required]
        [ForeignKey("CompanySubscription")]
        public int CompanySubscriptionId { get; set; }

        [Required]
        [ForeignKey("PaymentGateway")]
        public int PaymentGatewayId { get; set; }

        // ── References ────────────────────────────────────────────────────

        /// <summary>Your internal reference — generated before hitting gateway.</summary>
        [Required]
        [MaxLength(100)]
        public string InternalReference { get; set; } = string.Empty;

        /// <summary>
        /// Gateway's own transaction ID.
        /// e.g. Stripe PaymentIntent: pi_3N2abc...
        /// Required for gateway-side reconciliation and refund requests.
        /// </summary>
        [MaxLength(200)]
        public string? GatewayTransactionId { get; set; }

        // ── Payment Details ───────────────────────────────────────────────

        [Column(TypeName = "nvarchar(20)")]
        public PaymentMethod PaymentMethod { get; set; }

        [Required]
        [Column(TypeName = "decimal(19,4)")]
        public decimal Amount { get; set; }

        /// <summary>ISO 4217. Validated in service layer, not hardcoded regex.</summary>
        [Required]
        [MaxLength(3)]
        public string Currency { get; set; } = "USD";

        /// <summary>Gateway processing fee for internal reconciliation.</summary>
        [Column(TypeName = "decimal(19,4)")]
        public decimal GatewayFee { get; set; } = 0;

        /// <summary>Tax charged on this payment (VAT/GST).</summary>
        [Column(TypeName = "decimal(19,4)")]
        public decimal TaxAmount { get; set; } = 0;

        // ── Status ────────────────────────────────────────────────────────

        [Column(TypeName = "nvarchar(20)")]
        public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Initiated;

        [Column(TypeName = "nvarchar(30)")]
        public PaymentFailureCode FailureCode { get; set; } = PaymentFailureCode.None;

        [MaxLength(500)]
        public string? FailureMessage { get; set; }

        /// <summary>Raw JSON from gateway — store everything for debugging.</summary>
        [Column(TypeName = "nvarchar(max)")]
        public string? GatewayResponse { get; set; }

        /// <summary>Null until payment is actually completed.</summary>
        public DateTime? PaidAt { get; set; }

        // ── Refund Aggregate ──────────────────────────────────────────────
        // Detailed refund history lives in Refunds table.
        // These fields track the aggregate state for quick queries.

        [Column(TypeName = "decimal(19,4)")]
        public decimal RefundedAmount { get; set; } = 0;

        public DateTime? RefundedAt { get; set; }

        // ── Card Metadata (PCI-DSS Safe) ──────────────────────────────────

        /// <summary>Last 4 digits for display. e.g. "4242".</summary>
        [MaxLength(4)]
        public string? CardLastFour { get; set; }

        [Column(TypeName = "nvarchar(20)")]
        public CardBrand? CardBrand { get; set; }

        [MaxLength(2)]
        public string? CardExpiryMonth { get; set; }

        [MaxLength(4)]
        public string? CardExpiryYear { get; set; }

        /// <summary>
        /// Gateway token for saved card / auto-renewal.
        /// e.g. Stripe PaymentMethod ID: pm_1N2abc...
        /// </summary>
        [MaxLength(200)]
        public string? GatewayCardToken { get; set; }

        // ── Computed ──────────────────────────────────────────────────────
        [NotMapped]
        public bool IsSuccessful =>
            PaymentStatus == PaymentStatus.Success ||
            PaymentStatus == PaymentStatus.Captured;

        [NotMapped]
        public decimal NetAmount => Amount - GatewayFee - RefundedAmount;

        // ── Navigation ────────────────────────────────────────────────────
        public virtual CompanySubscription? CompanySubscription { get; set; }
        public virtual PaymentGateway? PaymentGateway { get; set; }
        public virtual Invoice? Invoice { get; set; }
        public virtual ICollection<PaymentAttempt> Attempts { get; set; }
            = new List<PaymentAttempt>();
        public virtual ICollection<Refund> Refunds { get; set; }
            = new List<Refund>();
    }
}