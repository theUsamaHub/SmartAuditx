// ─── IdempotencyKey.cs ────────────────────────────────────────────────────

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartAuditX.Models.BillingModule
{
    /// <summary>
    /// Prevents duplicate payment processing caused by:
    /// double-clicks, network retries, browser back button re-submits.
    /// Rule: before hitting the gateway, check if Key exists.
    /// If yes — return cached response. Never process twice.
    /// Keys expire after 24 hours — nightly cleanup job removes them.
    /// </summary>
    [Table("IdempotencyKeys")]
    public class IdempotencyKey :AuditableEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdempotencyKeyId { get; set; }

        /// <summary>UUID generated client-side per payment attempt.</summary>
        [Required]
        [MaxLength(100)]
        public string Key { get; set; } = string.Empty;

        [Required]
        [ForeignKey("Company")]
        public int CompanyId { get; set; }

        /// <summary>SHA256 hash of the request payload.</summary>
        [MaxLength(500)]
        public string? RequestHash { get; set; }

        /// <summary>
        /// Cached response returned on duplicate requests.
        /// Prevents double processing while giving the correct response.
        /// </summary>
        [Column(TypeName = "nvarchar(max)")]
        public string? ResponsePayload { get; set; }

        [Required]
        public DateTime ExpiresAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // ── Navigation ────────────────────────────────────────────────────
        public virtual Company? Company { get; set; }
    }
}