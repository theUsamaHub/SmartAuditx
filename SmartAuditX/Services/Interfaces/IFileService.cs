using SmartAuditX.Models.ViewModels;

namespace SmartAuditX.Services.Interfaces
{
    public interface IFileService
    {
        Task<FileUploadResult?> UploadCompanyLogoAsync(
            IFormFile? file);
    }
}