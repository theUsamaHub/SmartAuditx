using SmartAuditX.Models.AuditModule.AuditEnums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartAuditX.Models.AuditModule
{
    [Table("AuditBarcodeScans")]
    public class AuditBarcodeScan : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int AuditId { get; set; }

        [ForeignKey("AuditId")]
        public virtual Audit? Audit { get; set; }

        [ForeignKey("AuditResponse")]
        public int? AuditResponseId { get; set; }

        [Required]
        [MaxLength(100)]
        public string BarcodeValue { get; set; } = string.Empty;

        [MaxLength(255)]
        public string? ItemNameSnapshot { get; set; }

        [MaxLength(200)]
        public string? LocationSnapshot { get; set; }

        [MaxLength(100)]
        public string? SKUSnapshot { get; set; }

        [Column(TypeName = "decimal(10,4)")]
        public decimal? ExpectedQuantity { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,4)")]
        public decimal ActualQuantity { get; set; } = 0;

        [MaxLength(30)]
        public string? Unit { get; set; }

        [Column(TypeName = "decimal(10,4)")]
        public decimal? DiscrepancyQuantity { get; set; }

        public BarcodeScanStatus Status { get; set; } = BarcodeScanStatus.Unrecognized;

        [MaxLength(500)]
        public string? Notes { get; set; }

        public int ScanCount { get; set; } = 1;

        public DateTime FirstScannedAt { get; set; } = DateTime.UtcNow;
        public DateTime LastScannedAt { get; set; } = DateTime.UtcNow;

        public virtual AuditResponse? AuditResponse { get; set; }
    }
}
