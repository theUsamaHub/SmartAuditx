using SmartAuditX.Models.ViewModels;
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

        public async Task<FileUploadResult?> UploadCompanyLogoAsync(
            IFormFile? file)
        {
            // No image uploaded
            if (file == null || file.Length == 0)
            {
                return new FileUploadResult
                {
                    Success = true,
                    FilePath = null
                };
            }

            // ─────────────────────────────────────────────
            // VALIDATE FILE SIZE
            // MAX: 2 MB
            // ─────────────────────────────────────────────

            const long maxFileSize = 2 * 1024 * 1024;

            if (file.Length > maxFileSize)
            {
                return new FileUploadResult
                {
                    Success = false,
                    ErrorMessage = "Logo size cannot exceed 2 MB."
                };
            }

            // ─────────────────────────────────────────────
            // VALIDATE FILE EXTENSION
            // ─────────────────────────────────────────────

            var allowedExtensions = new[]
            {
                ".jpg",
                ".jpeg",
                ".png",
                ".webp" //we can remove this 
            };

            var extension =
                Path.GetExtension(file.FileName)
                    .ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
            {
                return new FileUploadResult
                {
                    Success = false,
                    ErrorMessage = "Only jpg,jpeg,webp allowed"
                };
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
                return new FileUploadResult
                {
                    Success = false,
                    ErrorMessage = "Invalid Image Format"
                };
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
            return new FileUploadResult
            {
                Success = true,
                FilePath = $"/CompanyLogo/{fileName}"
            };
        }

        public async Task<FileUploadResult> UploadFileAsync(IFormFile file, string folderName)
        {
            // Validate file
            if (file == null || file.Length == 0)
            {
                return new FileUploadResult
                {
                    Success = false,
                    ErrorMessage = "No file provided."
                };
            }

            // Validate file size (max 10MB)
            const long maxFileSize = 10 * 1024 * 1024;
            if (file.Length > maxFileSize)
            {
                return new FileUploadResult
                {
                    Success = false,
                    ErrorMessage = "File size cannot exceed 10 MB."
                };
            }

            // Validate file extension
            var allowedExtensions = new[] { ".pdf", ".jpg", ".jpeg", ".png", ".doc", ".docx", ".xls", ".xlsx" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            
            if (!allowedExtensions.Contains(extension))
            {
                return new FileUploadResult
                {
                    Success = false,
                    ErrorMessage = "Invalid file type. Allowed: PDF, JPG, PNG, DOC, DOCX, XLS, XLSX."
                };
            }

            // Create directory: wwwroot/{folderName}/
            var uploadsFolder = Path.Combine(_environment.WebRootPath, folderName);
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            // Generate secure file name
            var fileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            // Save file
            using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);

            // Return relative path for database
            return new FileUploadResult
            {
                Success = true,
                FilePath = $"/{folderName}/{fileName}",
                FileName = file.FileName,
                FileType = extension
            };
        }

        public Task<bool> DeleteFileAsync(string fileUrl)
        {
            if (string.IsNullOrWhiteSpace(fileUrl))
            {
                return Task.FromResult(false);
            }

            try
            {
                // Convert URL to physical path
                // e.g., "/EmployeeDocuments/abc123.pdf" -> "wwwroot/EmployeeDocuments/abc123.pdf"
                var relativePath = fileUrl.TrimStart('/', '~');
                var physicalPath = Path.Combine(_environment.WebRootPath, relativePath);

                if (File.Exists(physicalPath))
                {
                    File.Delete(physicalPath);
                    return Task.FromResult(true);
                }

                return Task.FromResult(false);
            }
            catch
            {
                // Log error in production
                return Task.FromResult(false);
            }
        }
    }
}