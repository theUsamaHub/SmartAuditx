using SmartAuditX.Models.AuditModule;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
namespace SmartAuditX.Models.ViewModels.AuditModule
{
    public class AuditTemplateViewModel
    {

        public int AuditTemplateId { get; set; }

        [Required]
        [MaxLength(150)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Description { get; set; }

        public bool IsScoringEnabled { get; set; } = true;

        public int Version { get; set; } = 1;

        public bool IsPublished { get; set; } = false;

        public List<AuditTemplateSectionViewModel> Sections { get; set; } = new List<AuditTemplateSectionViewModel>();
    }
}