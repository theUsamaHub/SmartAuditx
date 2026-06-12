using SmartAuditX.Models.ViewModels;

namespace SmartAuditX.Services.Interfaces
{
    public interface IFileService
    {
        Task<FileUploadResult?> UploadCompanyLogoAsync(
            IFormFile? file);

        /// <summary>
        /// Upload a file to a specific folder.
        /// </summary>
        /// <param name="file">The file to upload</param>
        /// <param name="folderName">Target folder (e.g., "EmployeeDocuments", "CompanyLogos")</param>
        /// <returns>FileUploadResult with file URL and metadata</returns>
        Task<FileUploadResult> UploadFileAsync(IFormFile file, string folderName);

        /// <summary>
        /// Delete a file by its URL/path.
        /// </summary>
        /// <param name="fileUrl">The file URL to delete</param>
        /// <returns>True if deleted successfully</returns>
        Task<bool> DeleteFileAsync(string fileUrl);
    }
}