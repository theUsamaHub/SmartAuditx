using SmartAuditX.Models.ViewModels;

namespace SmartAuditX.Services.Interfaces
{
    public interface IDepartmentService
    {
        Task<IReadOnlyList<DepartmentListItemViewModel>> GetAllAsync(
            int companyId,
            bool? isActive = null,
            string? search = null);

        Task<DepartmentViewModel?> GetForEditAsync(int companyId, int departmentId);

        Task<DepartmentOperationResult> CreateAsync(int companyId, DepartmentViewModel model);

        Task<DepartmentOperationResult> UpdateAsync(
            int companyId,
            int departmentId,
            DepartmentViewModel model);

        Task<DepartmentOperationResult> DeleteAsync(int companyId, int departmentId);

        Task<DepartmentOperationResult> ToggleActiveAsync(int companyId, int departmentId);
    }
}
