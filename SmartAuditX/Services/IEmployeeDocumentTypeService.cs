using SmartAuditX.Models;
using SmartAuditX.Models.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmartAuditX.Services.Interfaces
{
    public interface IEmployeeDocumentTypeService
    {
        Task<IEnumerable<EmployeeDocumentType>> GetAllAsync(int companyId);
        Task<EmployeeDocumentType?> GetByIdAsync(int id, int companyId);
        Task<EmployeeDocumentType> CreateAsync(EmployeeDocumentType model);
        Task<EmployeeDocumentType?> UpdateAsync(int id, EmployeeDocumentType model, int companyId);
        Task<bool> DeleteAsync(int id, int companyId);
        Task<bool> ToggleActiveStatusAsync(int id, int companyId);
        Task<bool> IsNameExistsAsync(string name, int companyId, int? excludeId = null);
        Task<(IEnumerable<EmployeeDocumentType> items, int totalCount)> GetPagedListAsync(
            int companyId,
            int pageNumber,
            int pageSize,
            string? searchTerm,
            bool? isActive,
            string? sortColumn,
            string? sortOrder);
    }
}