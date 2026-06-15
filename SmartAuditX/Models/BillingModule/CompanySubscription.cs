// ─── CompanySubscription.cs ───────────────────────────────────────────────

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SmartAuditX.Models.BillingModule.Enums;

namespace SmartAuditX.Models.BillingModule
{
    /// <summary>
    /// Tracks each company's active subscription lifecycle.
    /// Status enum handles all states: trial, active, grace period, suspended.
    /// Pause support: ExpiryDate extends by TotalPausedDays on resume.
    /// </summary>
    [Table("CompanySubscriptions")]
    public class CompanySubscription : AuditableEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CompanySubscriptionId { get; set; }

        [Required]
        [ForeignKey("Company")]
        public int CompanyId { get; set; }

        [Required]
        [ForeignKey("SubscriptionPlanPricing")]
        public int SubscriptionPlanPricingId { get; set; }

        [Column(TypeName = "nvarchar(20)")]
        public SubscriptionStatus Status { get; set; } = SubscriptionStatus.Pending;

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime ExpiryDate { get; set; }

        /// <summary>Set when plan has TrialDays > 0. Null = no trial.</summary>
        public DateTime? TrialEndsAt { get; set; }

        /// <summary>
        /// After payment failure, access continues until this date.
        /// Status = PastDue during grace period.
        /// After this → Status = Suspended.
        /// </summary>
        public DateTime? GracePeriodEndsAt { get; set; }

        /// <summary>Timestamp of when the company cancelled.</summary>
        public DateTime? CancelledAt { get; set; }

        public bool AutoRenew { get; set; } = true;

        /// <summary>Drives dunning retry decisions — stop retrying after threshold.</summary>
        public int RenewalAttemptCount { get; set; } = 0;

        // ── Pause Support ─────────────────────────────────────────────────
        public DateTime? PausedAt { get; set; }
        public DateTime? PauseUntil { get; set; }

        /// <summary>ExpiryDate is extended by this many days on resume.</summary>
        public int TotalPausedDays { get; set; } = 0;

        // ── Computed ──────────────────────────────────────────────────────
        [NotMapped]
        public bool IsExpired => DateTime.UtcNow > ExpiryDate;

        [NotMapped]
        public bool IsInTrial =>
            TrialEndsAt.HasValue && DateTime.UtcNow <= TrialEndsAt.Value;

        [NotMapped]
        public bool IsInGracePeriod =>
            GracePeriodEndsAt.HasValue && DateTime.UtcNow <= GracePeriodEndsAt.Value;

        [NotMapped]
        public int DaysRemaining => (ExpiryDate - DateTime.UtcNow).Days;

        // ── Navigation ────────────────────────────────────────────────────
        public virtual Company? Company { get; set; }
        //public virtual SubscriptionPlanPricing? SubscriptionPlanPricing { get; set; }
        public virtual ICollection<Payment> Payments { get; set; }
            = new List<Payment>();
        public virtual ICollection<Invoice> Invoices { get; set; }
            = new List<Invoice>();
        public virtual ICollection<DunningSchedule> DunningSchedules { get; set; }
            = new List<DunningSchedule>();
        public virtual ICollection<SubscriptionPlanChange> PlanChanges { get; set; }
            = new List<SubscriptionPlanChange>();
    }
}