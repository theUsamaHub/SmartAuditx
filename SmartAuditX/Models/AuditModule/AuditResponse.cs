using SmartAuditX.Models.AuditModule;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartAuditX.Models.AuditModule
{
    public class AuditResponse : AuditableEntity
    {
        [Required]
        public Guid AuditId { get; set; }

        [ForeignKey("AuditId")]
        public virtual Audit? Audit { get; set; }

        [Required]
        public Guid AuditTemplateItemId { get; set; }

        [ForeignKey("AuditTemplateItemId")]
        public virtual AuditTemplateItem? AuditTemplateItem { get; set; }

        [Required]
        public string AuditorId { get; set; } = string.Empty;

        [ForeignKey("AuditorId")]
        public virtual ApplicationUser? Auditor { get; set; }

        // String representation of the answer (e.g., "True", "32.5", "SelectedValue")
        public string? Value { get; set; }

        // Explicit pass/fail determination for auditing calculation
        public bool? IsPassed { get; set; }

        [MaxLength(1000)]
        public string? Comments { get; set; }

        public DateTimeOffset SubmittedAt { get; set; } = DateTimeOffset.UtcNow;

        // Navigation Properties
        public virtual ICollection<AuditEvidence> Evidences { get; set; } = new List<AuditEvidence>();
    }
}