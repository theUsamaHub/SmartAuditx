using System.ComponentModel.DataAnnotations;

namespace SmartAuditX.Models.ViewModels
{
    public class EmployeeDocumentViewModel
    {
        public int? EmployeeDocumentId { get; set; }
        
        [Required(ErrorMessage = "Document type is required.")]
        [Display(Name = "Document Type")]
        public int EmployeeDocumentTypeId { get; set; }

        public string? DocumentTypeName { get; set; }

        [Required(ErrorMessage = "File is required.")]
        [Display(Name = "Document File")]
        public IFormFile? File { get; set; }

        public string? FileUrl { get; set; }
        public string? FileName { get; set; }
        public string? FileType { get; set; }

        [Display(Name = "Verified")]
        public bool IsVerified { get; set; } = false;

        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    }
}
