namespace SmartAuditX.Services.Interfaces
{
    public interface IFileService
    {
        Task<string?> UploadCompanyLogoAsync(
            IFormFile? file);
    }
}