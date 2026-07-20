using SmartAuditX.Models.AuditModule.AuditEnums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartAuditX.Models.AuditModule
{
    public class AuditTemplateField
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int AuditTemplateSectionId { get; set; }

        [ForeignKey("AuditTemplateSectionId")]
        public virtual AuditTemplateSection? AuditTemplateSection { get; set; }

        /// <summary>Denormalized — stored directly for efficient field-level queries without joining back through Section.</summary>
        [Required]
        public int AuditTemplateId { get; set; }

        /// <summary>The question shown to the auditor. e.g. "Are fire exits unobstructed?"</summary>
        [Required]
        [MaxLength(500)]
        public string QuestionText { get; set; } = string.Empty;

        /// <summary>Guidance shown below the label. e.g. "Check all 3 exits on the ground floor."</summary>
        [MaxLength(1000)]
        public string? HelpText { get; set; }

        [Required]
        public TemplateItemType ItemType { get; set; }

        public int SortOrder { get; set; }
        public bool IsRequired { get; set; } = false;

        /// <summary>Points this field contributes toward the overall audit score.</summary>
        [Column(TypeName = "decimal(5,2)")]
        public decimal Weightage { get; set; } = 1.00m;

        /// <summary>For Number/Rating fields: minimum allowed value.</summary>
        [Column(TypeName = "decimal(10,4)")]
        public decimal? MinValue { get; set; }

        /// <summary>For Number/Rating fields: maximum allowed value.</summary>
        [Column(TypeName = "decimal(10,4)")]
        public decimal? MaxValue { get; set; }

        /// <summary>When true a notes/comment text box appears below this field.</summary>
        public bool AllowNotes { get; set; } = true;

        /// <summary>For Photo fields: minimum number of photos auditor must upload.</summary>
        public int? MinPhotoCount { get; set; }

        // Navigation Properties
        public virtual ICollection<AuditTemplateFieldOption> Options { get; set; } = new List<AuditTemplateFieldOption>();
    }
}
