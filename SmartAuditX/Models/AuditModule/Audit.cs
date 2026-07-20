using SmartAuditX.Models.AuditModule;
using SmartAuditX.Models.AuditModule.AuditEnums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartAuditX.Models.AuditModule
{
    public class Audit : AuditableEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int AuditTemplateId { get; set; }

        [ForeignKey("AuditTemplateId")]
        public virtual AuditTemplate? AuditTemplate { get; set; }

        [Required]
        public int CompanyId { get; set; }

        [ForeignKey("CompanyId")]
        public virtual Company? Company { get; set; }

        /// <summary>Optional relationship to audit a specific Branch</summary>
        public int? BranchId { get; set; }

        [ForeignKey("BranchId")]
        public virtual Branch? Branch { get; set; }

        /// <summary>Snapshot of AuditTemplate.Version at audit creation time.</summary>
        public int TemplateVersionSnapshot { get; set; }

        [Required]
        [MaxLength(150)]
        public string Title { get; set; } = string.Empty;

        public DateTimeOffset? ScheduledStartDate { get; set; }
        public DateTimeOffset? ScheduledEndDate { get; set; }

        public DateTimeOffset? ActualStartDate { get; set; }
        public DateTimeOffset? ActualEndDate { get; set; }

        [Required]
        public AuditStatus Status { get; set; } = AuditStatus.Draft;

        /// <summary>Percentage score 0.00 to 100.00. Null if scoring not enabled.</summary>
        [Column(TypeName = "decimal(5,2)")]
        public decimal? FinalScore { get; set; }

        /// <summary>General notes by auditor at the overall audit level.</summary>
        [MaxLength(2000)]
        public string? Notes { get; set; }

        /// <summary>Manager notes added during review.</summary>
        [MaxLength(2000)]
        public string? ReviewNotes { get; set; }

        /// <summary>Who the audit is assigned to (Auditor user ID).</summary>
        [ForeignKey("AssignedToUser")]
        public int? AssignedToUserId { get; set; }

        /// <summary>Who reviewed/approved the audit.</summary>
        [ForeignKey("ReviewedByUser")]
        public int? ReviewedByUserId { get; set; }

        public DateTimeOffset? ReviewedAt { get; set; }

        /// <summary>UserId of the person who created this audit.</summary>
        public int CreatedByUserId { get; set; }

        // Navigation Properties
        public virtual ApplicationUser? AssignedToUser { get; set; }
        public virtual ApplicationUser? ReviewedByUser { get; set; }
        public virtual ICollection<AuditAssignment> Assignments { get; set; } = new List<AuditAssignment>();
        public virtual ICollection<AuditResponse> Responses { get; set; } = new List<AuditResponse>();
    }
}
