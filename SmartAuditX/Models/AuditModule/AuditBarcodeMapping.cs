using SmartAuditX.Models.AuditModule.AuditEnums;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartAuditX.Models
{
    public class AuditBarcodeMapping : AuditableEntity
    {
        [Required]
        [MaxLength(100)]
        public string BarcodeValue { get; set; } = string.Empty;

        [Required]
        public int CompanyId { get; set; }

        [ForeignKey("CompanyId")]
        public virtual Company? Company { get; set; }

        [Required]
        public BarcodeMappingType MappingType { get; set; }

        // Holds the primary key target (e.g., Guid of AuditTemplate or AuditTemplateItem)
        [Required]
        public string TargetId { get; set; } = string.Empty;

        [MaxLength(250)]
        public string? Description { get; set; }
    }
}