// ─── PaymentGateway.cs ────────────────────────────────────────────────────

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SmartAuditX.Models.BillingModule.Enums;

namespace SmartAuditX.Models.BillingModule
{
    /// <summary>
    /// Master table of all payment gateways supported by the platform.
    /// Adding a new gateway (JazzCash, Razorpay, etc.) = adding one row.
    /// Zero schema changes or code migrations required.
    /// Gateway (WHO processes) is separate from PaymentMethod (HOW they pay).
    /// </summary>
    [Table("PaymentGateways")]
    public class PaymentGateway : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PaymentGatewayId { get; set; }

        /// <summary>Display name. e.g. "Stripe", "JazzCash", "EasyPaisa".</summary>
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Unique identifier used in code switch statements.
        /// e.g. "stripe", "jazzcash", "easypaisa".
        /// Lowercase, no spaces.
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string Slug { get; set; } = string.Empty;

        /// <summary>Local = PKR only. Global = multi-currency.</summary>
        [Required]
        [Column(TypeName = "nvarchar(10)")]
        public GatewayScope Scope { get; set; } = GatewayScope.Global;

        /// <summary>
        /// Comma-separated ISO 4217 currency codes this gateway supports.
        /// e.g. "PKR,USD" for JazzCash, "USD,EUR,GBP,PKR" for Stripe.
        /// </summary>
        [MaxLength(500)]
        public string SupportedCurrencies { get; set; } = string.Empty;

        /// <summary>
        /// Only one gateway should have IsDefault = true at a time.
        /// Used when no specific gateway is selected.
        /// </summary>
        public bool IsDefault { get; set; } = false;

        // ── Navigation ────────────────────────────────────────────────────
        public virtual ICollection<Payment> Payments { get; set; }
            = new List<Payment>();
        public virtual ICollection<WebhookLog> WebhookLogs { get; set; }
            = new List<WebhookLog>();
    }
}