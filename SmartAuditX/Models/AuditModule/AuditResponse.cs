using SmartAuditX.Models.AuditModule;
using SmartAuditX.Models.AuditModule.AuditEnums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartAuditX.Models.AuditModule
{
    public class AuditResponse : AuditableEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int AuditId { get; set; }

        [ForeignKey("AuditId")]
        public virtual Audit? Audit { get; set; }

        [Required]
        public int AuditTemplateFieldId { get; set; }

        [ForeignKey("AuditTemplateFieldId")]
        public virtual AuditTemplateField? AuditTemplateField { get; set; }

        // Snapshots (protect historical accuracy when template is edited)
        [Required]
        [MaxLength(500)]
        public string FieldLabelSnapshot { get; set; } = string.Empty;

        public TemplateItemType FieldTypeSnapshot { get; set; }

        // Response Values (only one populated per row based on FieldType)
        [MaxLength(2000)]
        public string? ResponseText { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal? ResponseNumber { get; set; }

        public bool? ResponseBoolean { get; set; }

        public DateTime? ResponseDate { get; set; }

        /// <summary>For Dropdown fields — which option the auditor selected.</summary>
        [ForeignKey("SelectedOption")]
        public int? SelectedOptionId { get; set; }

        public virtual AuditTemplateFieldOption? SelectedOption { get; set; }

        // Scoring
        [Column(TypeName = "decimal(10,2)")]
        public decimal? Score { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }

        /// <summary>True if field was optional and auditor left it blank.</summary>
        public bool IsSkipped { get; set; } = false;

        // Navigation Properties
        public virtual ICollection<AuditEvidence> Evidences { get; set; } = new List<AuditEvidence>();
    }
}
