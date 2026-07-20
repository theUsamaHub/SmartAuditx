using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartAuditX.Models.AuditModule
{
    [Table("AuditTemplateInventoryItems")]
    public class AuditTemplateInventoryItem : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int AuditTemplateId { get; set; }

        [ForeignKey("AuditTemplateId")]
        public virtual AuditTemplate? AuditTemplate { get; set; }

        [Required]
        [MaxLength(100)]
        public string BarcodeValue { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        public string ItemName { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? Location { get; set; }

        [MaxLength(100)]
        public string? SKU { get; set; }

        [MaxLength(100)]
        public string? Category { get; set; }

        [Column(TypeName = "decimal(10,4)")]
        public decimal ExpectedQuantity { get; set; }

        [MaxLength(30)]
        public string? Unit { get; set; }
    }
}
