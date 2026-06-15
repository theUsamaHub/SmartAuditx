using SmartAuditX.Models.AuditModule;
using SmartAuditX.Models.AuditModule.AuditEnums;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartAuditX.Models.AuditModule
{
    public class AuditTemplateItem : BaseEntity
    {
        [Required]
        public Guid AuditTemplateId { get; set; }

        [ForeignKey("AuditTemplateId")]
        public virtual AuditTemplate? AuditTemplate { get; set; }

        [Required]
        [MaxLength(100)]
        public string SectionName { get; set; } = "General"; // Used for grouping UI fields

        public int SortOrder { get; set; }

        [Required]
        [MaxLength(500)]
        public string QuestionText { get; set; } = string.Empty;

        [Required]
        public TemplateItemType ItemType { get; set; }

        public bool IsRequired { get; set; } = false;

        // Weightage for scoring math
        [Column(TypeName = "decimal(5,2)")]
        public decimal Weightage { get; set; } = 1.00m;

        // Stores serialized options for dropdowns (e.g., ["High", "Medium", "Low"])
        public string? ConfigurationJson { get; set; }
    }
}