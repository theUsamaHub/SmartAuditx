// ─── Invoice.cs ───────────────────────────────────────────────────────────

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SmartAuditX.Models.BillingModule.Enums;

namespace SmartAuditX.Models.BillingModule
{
    /// <summary>
    /// Formal invoice for B2B billing and compliance.
    /// Can exist as Draft before payment (proforma) or Paid after.
    /// InvoiceNumber is sequential and never reused — required for auditing.
    /// Format: INV-2025-000001.
    /// </summary>
    [Table("Invoices")]
    public class Invoice : AuditableEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int InvoiceId { get; set; }

        [Required]
        [ForeignKey("CompanySubscription")]
        public int CompanySubscriptionId { get; set; }

        /// <summary>Null if invoice is a draft (pre-payment).</summary>
        [ForeignKey("Payment")]
        public int? PaymentId { get; set; }

        [Required]
        [MaxLength(50)]
        public string InvoiceNumber { get; set; } = string.Empty;

        [Required]
        public DateTime IssuedAt { get; set; } = DateTime.UtcNow;

        public DateTime? DueDate { get; set; }

        [Required]
        [Column(TypeName = "decimal(19,4)")]
        public decimal SubTotal { get; set; }

        [Column(TypeName = "decimal(19,4)")]
        public decimal TaxAmount { get; set; } = 0;

        [Column(TypeName = "decimal(19,4)")]
        public decimal Discount { get; set; } = 0;

        [Required]
        [Column(TypeName = "decimal(19,4)")]
        public decimal TotalAmount { get; set; }

        [Required]
        [MaxLength(3)]
        public string Currency { get; set; } = "USD";

        [Column(TypeName = "nvarchar(10)")]
        public InvoiceStatus Status { get; set; } = InvoiceStatus.Draft;

        /// <summary>URL to generated PDF in cloud storage.</summary>
        [MaxLength(500)]
        public string? PdfUrl { get; set; }

        // ── Navigation ────────────────────────────────────────────────────
        public virtual CompanySubscription? CompanySubscription { get; set; }
        public virtual Payment? Payment { get; set; }
    }
}