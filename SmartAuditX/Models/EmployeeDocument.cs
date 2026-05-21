using System.ComponentModel.DataAnnotations;

namespace SmartAuditX.Models
{
    public class EmployeeDocument
    {
        [Key]
        public int EmployeeDocumentId { get; set; }

        [Required]
        public int EmployeeId { get; set; }

        [Required]
        public int EmployeeDocumentTypeId { get; set; }

        [Required]
        [MaxLength(500)]
        public string FileUrl { get; set; } = string.Empty;

        [MaxLength(255)]
        public string? FileName { get; set; }

        [MaxLength(50)]
        public string? FileType { get; set; }

        public bool IsVerified { get; set; } = false;

        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Snapshot (prevents future rename breaking history)
        [MaxLength(150)]
        public string? DocumentTypeNameSnapshot { get; set; }

        public Employee? Employee { get; set; }
        public EmployeeDocumentType? EmployeeDocumentType { get; set; }
    }
}
