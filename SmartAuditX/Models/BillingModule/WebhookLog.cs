// ─── WebhookLog.cs ────────────────────────────────────────────────────────

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SmartAuditX.Models.BillingModule.Enums;

namespace SmartAuditX.Models.BillingModule
{
    /// <summary>
    /// Logs all incoming webhook events from payment gateways.
    /// CRITICAL: Without this, if payment succeeds at gateway but your server
    /// crashes before updating the DB — you have no recovery mechanism.
    /// GatewayEventId enables deduplication — gateways retry on timeout.
    /// Rule: always check GatewayEventId before processing any webhook.
    /// </summary>
    [Table("WebhookLogs")]
    public class WebhookLog : AuditableEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int WebhookLogId { get; set; }

        [Required]
        [ForeignKey("PaymentGateway")]
        public int PaymentGatewayId { get; set; }

        /// <summary>e.g. "payment.success", "refund.created", "dispute.opened".</summary>
        [Required]
        [MaxLength(100)]
        public string EventType { get; set; } = string.Empty;

        /// <summary>
        /// Gateway's own event ID for deduplication.
        /// Check this before processing — gateways send duplicates on timeout.
        /// </summary>
        [MaxLength(200)]
        public string? GatewayEventId { get; set; }

        /// <summary>Full raw JSON body — store everything, always.</summary>
        [Required]
        [Column(TypeName = "nvarchar(max)")]
        public string Payload { get; set; } = string.Empty;

        [Column(TypeName = "nvarchar(10)")]
        public WebhookStatus Status { get; set; } = WebhookStatus.Received;

        [MaxLength(500)]
        public string? FailureReason { get; set; }

        public DateTime ReceivedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ProcessedAt { get; set; }

        // ── Navigation ────────────────────────────────────────────────────
        public virtual PaymentGateway? PaymentGateway { get; set; }
    }
}