// ─── PaymentNotification.cs ───────────────────────────────────────────────

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SmartAuditX.Models.BillingModule.Enums;

namespace SmartAuditX.Models.BillingModule
{
    /// <summary>
    /// Logs every payment-related notification sent to companies.
    /// Prevents spam: check log before sending duplicate alerts.
    /// Provides debugging trail when company claims they never received an email.
    /// </summary>
    [Table("PaymentNotifications")]
    public class PaymentNotification : AuditableEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PaymentNotificationId { get; set; }

        [Required]
        [ForeignKey("Company")]
        public int CompanyId { get; set; }

        [Column(TypeName = "nvarchar(25)")]
        public NotificationType Type { get; set; }

        /// <summary>Email | SMS | Both.</summary>
        [Required]
        [MaxLength(10)]
        public string Channel { get; set; } = "Email";

        public DateTime SentAt { get; set; } = DateTime.UtcNow;

        /// <summary>Sent | Delivered | Failed | Bounced.</summary>
        [Required]
        [MaxLength(15)]
        public string DeliveryStatus { get; set; } = "Sent";

        // ── Navigation ────────────────────────────────────────────────────
        public virtual Company? Company { get; set; }
    }
}