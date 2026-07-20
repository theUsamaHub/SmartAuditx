using SmartAuditX.Models.AuditModule.AuditEnums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartAuditX.Models.ViewModels.AuditModule
{
    public class AuditTemplateFieldViewModel
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(500)]
        public string QuestionText { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? HelpText { get; set; }

        [Required]
        public TemplateItemType ItemType { get; set; }

        public int SortOrder { get; set; }

        public bool IsRequired { get; set; } = false;

        [Column(TypeName = "decimal(5,2)")]
        public decimal Weightage { get; set; } = 1.00m;

        [Column(TypeName = "decimal(10,4)")]
        public decimal? MinValue { get; set; }

        [Column(TypeName = "decimal(10,4)")]
        public decimal? MaxValue { get; set; }

        public bool AllowNotes { get; set; } = true;

        public List<AuditTemplateFieldOptionViewModel> Options { get; set; } = new List<AuditTemplateFieldOptionViewModel>();
    }
}
