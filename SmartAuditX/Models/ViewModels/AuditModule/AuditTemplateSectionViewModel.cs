using SmartAuditX.Models.AuditModule;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
namespace SmartAuditX.Models.ViewModels.AuditModule
{
    public class AuditTemplateSectionViewModel
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(150)]
        public string Title { get; set; } = string.Empty;

        public int SortOrder { get; set; }

        public List<AuditTemplateFieldViewModel> Fields { get; set; } = new List<AuditTemplateFieldViewModel>();
    }
}