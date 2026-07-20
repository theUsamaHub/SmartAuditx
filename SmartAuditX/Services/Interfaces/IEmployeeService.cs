using SmartAuditX.Models.ViewModels;

namespace SmartAuditX.Services.Interfaces
{
    public interface IEmployeeService
    {
        Task<IReadOnlyList<EmployeeListItemViewModel>> GetAllAsync(
            int companyId,
            int? branchId = null,
            int? departmentId = null,
            int? designationId = null,
            bool? isActive = null,
            string? search = null);

        Task<EmployeeViewModel?> GetForEditAsync(int companyId, int employeeId);

        Task<EmployeeOperationResult> CreateAsync(int companyId, EmployeeViewModel model);

        Task<EmployeeOperationResult> UpdateAsync(int companyId, int employeeId, EmployeeViewModel model);

        Task<EmployeeOperationResult> DeleteAsync(int companyId, int employeeId);

        Task<EmployeeOperationResult> ToggleActiveAsync(int companyId, int employeeId);

        Task<EmployeeOperationResult> CreateSystemUserAsync(int companyId, int employeeId, CreateSystemUserViewModel model);
        Task<EmployeeOperationResult> RemoveSystemUserAsync(int companyId, int employeeId);
    }
}
