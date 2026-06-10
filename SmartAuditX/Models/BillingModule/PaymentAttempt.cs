// ─── PaymentAttempt.cs ────────────────────────────────────────────────────

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SmartAuditX.Models.BillingModule.Enums;

namespace SmartAuditX.Models.BillingModule
{
    /// <summary>
    /// Tracks every individual attempt for a single Payment record.
    /// User fails 3 times then succeeds — all 4 attempts are recorded here.
    /// Critical for debugging: "Why did 40% of payments fail on Tuesday?"
    /// </summary>
    [Table("PaymentAttempts")]
    public class PaymentAttempt : AuditableEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PaymentAttemptId { get; set; }

        [Required]
        [ForeignKey("Payment")]
        public int PaymentId { get; set; }

        [Required]
        [ForeignKey("PaymentGateway")]
        public int PaymentGatewayId { get; set; }

        /// <summary>Sequential: 1, 2, 3...</summary>
        public int AttemptNumber { get; set; } = 1;

        [Column(TypeName = "nvarchar(20)")]
        public PaymentStatus Status { get; set; } = PaymentStatus.Initiated;

        [Column(TypeName = "nvarchar(30)")]
        public PaymentFailureCode FailureCode { get; set; } = PaymentFailureCode.None;

        [MaxLength(500)]
        public string? FailureMessage { get; set; }

        [Column(TypeName = "nvarchar(max)")]
        public string? GatewayResponse { get; set; }

        /// <summary>Gateway's own ID for this specific attempt.</summary>
        [MaxLength(200)]
        public string? GatewayTransactionId { get; set; }

        public DateTime AttemptedAt { get; set; } = DateTime.UtcNow;
        public DateTime? CompletedAt { get; set; }

        // ── Navigation ────────────────────────────────────────────────────
        public virtual Payment? Payment { get; set; }
        public virtual PaymentGateway? PaymentGateway { get; set; }
    }
}