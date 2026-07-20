using SmartAuditX.Models.AuditModule.AuditEnums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SmartAuditX.Models.ViewModels.AuditModule
{
    public class AuditResponseViewModel
    {
        public int Id { get; set; }

        public int AuditId { get; set; }

        public int AuditTemplateFieldId { get; set; }

        public string? FieldLabelSnapshot { get; set; }

        public TemplateItemType FieldTypeSnapshot { get; set; }

        public string? ResponseText { get; set; }

        public decimal? ResponseNumber { get; set; }

        public bool? ResponseBoolean { get; set; }

        public DateTime? ResponseDate { get; set; }

        public int? SelectedOptionId { get; set; }

        public decimal? Score { get; set; }

        public string? Notes { get; set; }

        public bool IsSkipped { get; set; } = false;
    }

    public class AuditConductViewModel
    {
        public int AuditId { get; set; }
        public string? AuditTitle { get; set; }
        public string? TemplateTitle { get; set; }
        public AuditStatus Status { get; set; }
        public DateTimeOffset? ScheduledStartDate { get; set; }

        public List<AuditSectionConductViewModel> Sections { get; set; } = new List<AuditSectionConductViewModel>();
    }

    public class AuditSectionConductViewModel
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public int SortOrder { get; set; }
        public List<AuditFieldConductViewModel> Fields { get; set; } = new List<AuditFieldConductViewModel>();
    }

    public class AuditFieldConductViewModel
    {
        public int Id { get; set; }
        public string? QuestionText { get; set; }
        public string? HelpText { get; set; }
        public TemplateItemType ItemType { get; set; }
        public bool IsRequired { get; set; }
        public decimal Weightage { get; set; }
        public decimal? MinValue { get; set; }
        public decimal? MaxValue { get; set; }
        public bool AllowNotes { get; set; }
        public List<AuditFieldOptionConductViewModel> Options { get; set; } = new List<AuditFieldOptionConductViewModel>();

        // Response values
        public string? ResponseText { get; set; }
        public decimal? ResponseNumber { get; set; }
        public bool? ResponseBoolean { get; set; }
        public DateTime? ResponseDate { get; set; }
        public int? SelectedOptionId { get; set; }
        public string? Notes { get; set; }
        public bool IsSkipped { get; set; }
    }

    public class AuditFieldOptionConductViewModel
    {
        public int Id { get; set; }
        public string? Text { get; set; }
    }
}
