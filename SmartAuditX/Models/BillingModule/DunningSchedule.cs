// ─── DunningSchedule.cs ───────────────────────────────────────────────────

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SmartAuditX.Models.BillingModule.Enums;

namespace SmartAuditX.Models.BillingModule
{
    /// <summary>
    /// Manages automated retry logic when subscription renewal fails.
    /// Standard pattern: Fail → retry Day 1 → Day 3 → Day 7 → Suspend.
    /// Each row = one scheduled retry attempt.
    /// Background job reads Pending rows and executes them at ScheduledAt.
    /// </summary>
    [Table("DunningSchedules")]
    public class DunningSchedule : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int DunningScheduleId { get; set; }

        [Required]
        [ForeignKey("CompanySubscription")]
        public int CompanySubscriptionId { get; set; }

        /// <summary>Retry sequence number: 1st, 2nd, 3rd attempt.</summary>
        public int AttemptNumber { get; set; } = 1;

        [Required]
        public DateTime ScheduledAt { get; set; }

        public DateTime? AttemptedAt { get; set; }

        [Column(TypeName = "nvarchar(15)")]
        public DunningStatus Status { get; set; } = DunningStatus.Pending;

        [ForeignKey("PaymentAttempt")]
        public int? PaymentAttemptId { get; set; }

        // ── Navigation ────────────────────────────────────────────────────
        public virtual CompanySubscription? CompanySubscription { get; set; }
        public virtual PaymentAttempt? PaymentAttempt { get; set; }
    }
}