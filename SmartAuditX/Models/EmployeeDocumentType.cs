using System.ComponentModel.DataAnnotations;

namespace SmartAuditX.Models
{
    public class EmployeeDocumentType
    {
        [Key]
        public int EmployeeDocumentTypeId { get; set; }

        [Required]
        public int CompanyId { get; set; }

        [Required]
        [MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        public bool IsRequired { get; set; } = false;

        [MaxLength(500)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public bool IsDeleted { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public Company? Company { get; set; }
    }
}
