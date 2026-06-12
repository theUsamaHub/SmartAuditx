using SmartAuditX.Models;
using SmartAuditX.Models.ViewModels;

namespace SmartAuditX.Services.Interfaces
{
    public interface IEmployeeDocumentService
    {
        Task<IEnumerable<EmployeeDocument>> GetByEmployeeAsync(int employeeId, int companyId);

        Task<EmployeeDocument?> GetByIdAsync(int documentId, int companyId);

        Task<(bool success, string message, EmployeeDocument? document)> UploadAsync(
            int employeeId,
            int companyId,
            EmployeeDocumentViewModel model);

        Task<(bool success, string message)> DeleteAsync(int documentId, int companyId);

        Task<(bool success, string message)> ToggleVerifiedAsync(int documentId, int companyId);
    }
}
