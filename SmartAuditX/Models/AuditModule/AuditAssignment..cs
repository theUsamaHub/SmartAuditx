using SmartAuditX.Models.AuditModule;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartAuditX.Models.AuditModule
{
    public class AuditAssignment
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int AuditId { get; set; }

        [ForeignKey("AuditId")]
        public virtual Audit? Audit { get; set; }

        [Required]
        public int AuditorId { get; set; } // References ASP.NET Core Identity's ApplicationUser

        [ForeignKey("AuditorId")]
        public virtual ApplicationUser? Auditor { get; set; }

        public DateTimeOffset AssignedAt { get; set; } = DateTimeOffset.UtcNow;
    }
}