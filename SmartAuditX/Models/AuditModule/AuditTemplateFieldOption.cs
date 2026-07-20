using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartAuditX.Models.AuditModule
{
    public class AuditTemplateFieldOption
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int AuditTemplateFieldId { get; set; }

        [ForeignKey("AuditTemplateFieldId")]
        public virtual AuditTemplateField? AuditTemplateField { get; set; }

        [Required]
        [MaxLength(200)]
        public string Text { get; set; } = string.Empty;

        public int SortOrder { get; set; }

        // Optional value if the option has an underlying value (e.g., for scoring)
        public string? Value { get; set; }
    }
}