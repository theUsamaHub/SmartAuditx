using Microsoft.EntityFrameworkCore;
using SmartAuditX.Data;
using SmartAuditX.Models;
using SmartAuditX.Models.ViewModels;
using SmartAuditX.Services.Interfaces;

namespace SmartAuditX.Services.Implementations
{
    public class EmployeeDocumentService : IEmployeeDocumentService
    {
        private readonly ApplicationDbContext _context;
        private readonly IFileService _fileService;

        public EmployeeDocumentService(
            ApplicationDbContext context,
            IFileService fileService)
        {
            _context = context;
            _fileService = fileService;
        }

        public async Task<IEnumerable<EmployeeDocument>> GetByEmployeeAsync(int employeeId, int companyId)
        {
            // Verify employee belongs to company
            var employeeExists = await _context.Employees
                .AnyAsync(e => e.EmployeeId == employeeId && e.CompanyId == companyId && !e.IsDeleted);

            if (!employeeExists)
                return Enumerable.Empty<EmployeeDocument>();

            return await _context.EmployeeDocuments
                .Where(d => d.EmployeeId == employeeId)
                .Include(d => d.EmployeeDocumentType)
                .OrderByDescending(d => d.UploadedAt)
                .ToListAsync();
        }

        public async Task<EmployeeDocument?> GetByIdAsync(int documentId, int companyId)
        {
            return await _context.EmployeeDocuments
                .Include(d => d.Employee)
                .Include(d => d.EmployeeDocumentType)
                .FirstOrDefaultAsync(d =>
                    d.EmployeeDocumentId == documentId &&
                    d.Employee != null &&
                    d.Employee.CompanyId == companyId);
        }

        public async Task<(bool success, string message, EmployeeDocument? document)> UploadAsync(
            int employeeId,
            int companyId,
            EmployeeDocumentViewModel model)
        {
            // Verify employee belongs to company
            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.EmployeeId == employeeId && e.CompanyId == companyId && !e.IsDeleted);

            if (employee == null)
                return (false, "Employee not found.", null);

            // Validate document type exists for this company
            var docType = await _context.EmployeeDocumentTypes
                .FirstOrDefaultAsync(dt =>
                    dt.EmployeeDocumentTypeId == model.EmployeeDocumentTypeId &&
                    dt.CompanyId == companyId &&
                    !dt.IsDeleted);

            if (docType == null)
                return (false, "Invalid document type.", null);

            // Validate file upload
            if (model.File == null || model.File.Length == 0)
                return (false, "Please select a file to upload.", null);

            // Check file size (max 10MB)
            if (model.File.Length > 10 * 1024 * 1024)
                return (false, "File size must be less than 10MB.", null);

            // Validate file extension
            var allowedExtensions = new[] { ".pdf", ".jpg", ".jpeg", ".png", ".doc", ".docx", ".xls", ".xlsx" };
            var extension = Path.GetExtension(model.File.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension))
                return (false, "Invalid file type. Allowed: PDF, JPG, PNG, DOC, DOCX, XLS, XLSX.", null);

            // Upload file using FileService
            var uploadResult = await _fileService.UploadFileAsync(model.File, "EmployeeDocuments");
            if (!uploadResult.Success)
                return (false, uploadResult.ErrorMessage, null);

            // Create EmployeeDocument
            var document = new EmployeeDocument
            {
                EmployeeId = employeeId,
                EmployeeDocumentTypeId = model.EmployeeDocumentTypeId,
                FileUrl = uploadResult.FileUrl!,
                FileName = model.File.FileName,
                FileType = extension,
                DocumentTypeNameSnapshot = docType.Name,
                IsVerified = model.IsVerified,
                UploadedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };

            _context.EmployeeDocuments.Add(document);
            await _context.SaveChangesAsync();

            return (true, "Document uploaded successfully.", document);
        }

        public async Task<(bool success, string message)> DeleteAsync(int documentId, int companyId)
        {
            var document = await _context.EmployeeDocuments
                .Include(d => d.Employee)
                .FirstOrDefaultAsync(d =>
                    d.EmployeeDocumentId == documentId &&
                    d.Employee != null &&
                    d.Employee.CompanyId == companyId);

            if (document == null)
                return (false, "Document not found.");

            // Delete file from storage
            if (!string.IsNullOrWhiteSpace(document.FileUrl))
            {
                await _fileService.DeleteFileAsync(document.FileUrl);
            }

            _context.EmployeeDocuments.Remove(document);
            await _context.SaveChangesAsync();

            return (true, "Document deleted successfully.");
        }

        public async Task<(bool success, string message)> ToggleVerifiedAsync(int documentId, int companyId)
        {
            var document = await _context.EmployeeDocuments
                .Include(d => d.Employee)
                .FirstOrDefaultAsync(d =>
                    d.EmployeeDocumentId == documentId &&
                    d.Employee != null &&
                    d.Employee.CompanyId == companyId);

            if (document == null)
                return (false, "Document not found.");

            document.IsVerified = !document.IsVerified;
            await _context.SaveChangesAsync();

            var message = document.IsVerified
                ? "Document marked as verified."
                : "Document marked as unverified.";

            return (true, message);
        }
    }
}
