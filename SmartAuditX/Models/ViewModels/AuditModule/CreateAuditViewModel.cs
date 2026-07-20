using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SmartAuditX.Models.ViewModels.AuditModule
{
    public class CreateAuditViewModel
    {
        [Required]
        public int AuditTemplateId { get; set; }

        [Required]
        [MaxLength(150)]
        public string Title { get; set; } = string.Empty;

        public int? BranchId { get; set; }

        [Required]
        public DateTimeOffset ScheduledStartDate { get; set; }

        public DateTimeOffset? ScheduledEndDate { get; set; }

        public int? AssignedToUserId { get; set; }

        public string? Notes { get; set; }

        // UI dropdowns
        public SelectList? Templates { get; set; }
        public SelectList? Branches { get; set; }
        public SelectList? Auditors { get; set; }
    }
}
