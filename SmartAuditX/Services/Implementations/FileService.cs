using SmartAuditX.Services.Interfaces;

namespace SmartAuditX.Services.Implementations
{
    public class FileService : IFileService
    {
        private readonly IWebHostEnvironment _environment;

        public FileService(
            IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        public async Task<string?> UploadCompanyLogoAsync(
            IFormFile? file)
        {
            // No image uploaded
            if (file == null || file.Length == 0)
            {
                return null;
            }

            // ─────────────────────────────────────────────
            // VALIDATE FILE SIZE
            // MAX: 2 MB
            // ─────────────────────────────────────────────

            const long maxFileSize = 2 * 1024 * 1024;

            if (file.Length > maxFileSize)
            {
                throw new Exception(
                    "Logo size cannot exceed 2 MB.");
            }

            // ─────────────────────────────────────────────
            // VALIDATE FILE EXTENSION
            // ─────────────────────────────────────────────

            var allowedExtensions = new[]
            {
                ".jpg",
                ".jpeg",
                ".png",
                ".webp"
            };

            var extension =
                Path.GetExtension(file.FileName)
                    .ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
            {
                throw new Exception(
                    "Only JPG, JPEG, PNG, and WEBP images are allowed.");
            }

            // ─────────────────────────────────────────────
            // VALIDATE MIME TYPE
            // ─────────────────────────────────────────────

            var allowedMimeTypes = new[]
            {
                "image/jpeg",
                "image/png",
                "image/webp"
            };

            if (!allowedMimeTypes.Contains(file.ContentType))
            {
                throw new Exception(
                    "Invalid image format.");
            }

            // ─────────────────────────────────────────────
            // CREATE DIRECTORY
            // wwwroot/CompanyLogo/
            // ─────────────────────────────────────────────

            var uploadsFolder =
                Path.Combine(
                    _environment.WebRootPath,
                    "CompanyLogo");

            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            // ─────────────────────────────────────────────
            // GENERATE SECURE FILE NAME
            // ─────────────────────────────────────────────

            var fileName =
                $"{Guid.NewGuid()}{extension}";

            var filePath =
                Path.Combine(
                    uploadsFolder,
                    fileName);

            // ─────────────────────────────────────────────
            // SAVE FILE
            // ─────────────────────────────────────────────

            using var stream =
                new FileStream(
                    filePath,
                    FileMode.Create);

            await file.CopyToAsync(stream);

            // Return relative path for database
            return $"/CompanyLogo/{fileName}";
        }
    }
}