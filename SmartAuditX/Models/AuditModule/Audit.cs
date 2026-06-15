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
        [Required]
        public Guid AuditTemplateId { get; set; }

        [ForeignKey("AuditTemplateId")]
        public virtual AuditTemplate? AuditTemplate { get; set; }

        [Required]
        public int CompanyId { get; set; }

        [ForeignKey("CompanyId")]
        public virtual Company? Company { get; set; }

        // Optional relationship to audit a specific Branch
        public int? BranchId { get; set; }

        [ForeignKey("BranchId")]
        public virtual Branch? Branch { get; set; }

        [Required]
        [MaxLength(150)]
        public string Title { get; set; } = string.Empty;

        public DateTimeOffset? ScheduledStartDate { get; set; }
        public DateTimeOffset? ScheduledEndDate { get; set; }

        public DateTimeOffset? ActualStartDate { get; set; }
        public DateTimeOffset? ActualEndDate { get; set; }

        [Required]
        public AuditStatus Status { get; set; } = AuditStatus.Draft;

        // Calculated and cached upon completion
        [Column(TypeName = "decimal(5,2)")]
        public decimal? FinalScore { get; set; }

        // Navigation Properties
        public virtual ICollection<AuditAssignment> Assignments { get; set; } = new List<AuditAssignment>();
        public virtual ICollection<AuditResponse> Responses { get; set; } = new List<AuditResponse>();
    }
}