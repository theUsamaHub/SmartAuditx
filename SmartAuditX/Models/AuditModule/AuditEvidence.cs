using SmartAuditX.Models.AuditModule;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartAuditX.Models
{
    public class AuditEvidence : AuditableEntity
    {
        [Required]
        public Guid AuditResponseId { get; set; }

        [ForeignKey("AuditResponseId")]
        public virtual AuditResponse? AuditResponse { get; set; }

        [Required]
        [MaxLength(255)]
        public string FilePath { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string FileName { get; set; } = string.Empty;

        [MaxLength(50)]
        public string FileExtension { get; set; } = string.Empty;

        public long FileSizeBytes { get; set; }
    }
}