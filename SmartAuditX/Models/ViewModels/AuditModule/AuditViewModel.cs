using SmartAuditX.Models.AuditModule.AuditEnums;
using System;
using System.ComponentModel.DataAnnotations;

namespace SmartAuditX.Models.ViewModels.AuditModule
{
    public class AuditViewModel
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(150)]
        public string Title { get; set; } = string.Empty;

        public int AuditTemplateId { get; set; }

        public string? TemplateTitle { get; set; }

        public int CompanyId { get; set; }

        public int? BranchId { get; set; }

        public string? BranchName { get; set; }

        public AuditStatus Status { get; set; } = AuditStatus.Draft;

        public DateTimeOffset? ScheduledStartDate { get; set; }

        public DateTimeOffset? ScheduledEndDate { get; set; }

        public DateTimeOffset? ActualStartDate { get; set; }

        public DateTimeOffset? ActualEndDate { get; set; }

        public decimal? FinalScore { get; set; }

        public int? AssignedToUserId { get; set; }

        public string? AssignedToUserName { get; set; }

        public int? ReviewedByUserId { get; set; }

        public string? ReviewedByUserName { get; set; }

        public DateTimeOffset? ReviewedAt { get; set; }

        public string? Notes { get; set; }

        public string? ReviewNotes { get; set; }

        public int CreatedByUserId { get; set; }

        public DateTimeOffset CreatedAt { get; set; }
    }
}
