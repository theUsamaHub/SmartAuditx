namespace SmartAuditX.Models.ViewModels
{
    public class FileUploadResult
    {
        public bool Success { get; set; }

        public string? FilePath { get; set; }

        public string? ErrorMessage { get; set; }
    }
}
