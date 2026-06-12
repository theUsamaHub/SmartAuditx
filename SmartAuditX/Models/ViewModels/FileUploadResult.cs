namespace SmartAuditX.Models.ViewModels
{
    public class FileUploadResult
    {
        public bool Success { get; set; }

        public string? FilePath { get; set; }

        public string? ErrorMessage { get; set; }

        public string? FileName { get; set; }

        public string? FileType { get; set; }

        /// <summary>
        /// The file URL (same as FilePath but more explicit for API responses)
        /// </summary>
        public string? FileUrl => FilePath;
    }
}
