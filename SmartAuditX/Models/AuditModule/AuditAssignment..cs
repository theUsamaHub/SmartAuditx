using SmartAuditX.Models.AuditModule;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartAuditX.Models.AuditModule
{
    public class AuditAssignment
    {
        [Required]
        public Guid AuditId { get; set; }

        [ForeignKey("AuditId")]
        public virtual Audit? Audit { get; set; }

        [Required]
        public string AuditorId { get; set; } = string.Empty; // References ASP.NET Core Identity's ApplicationUser

        [ForeignKey("AuditorId")]
        public virtual ApplicationUser? Auditor { get; set; }

        public DateTimeOffset AssignedAt { get; set; } = DateTimeOffset.UtcNow;
    }
}