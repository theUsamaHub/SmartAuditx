using SmartAuditX.Models.ViewModels;

namespace SmartAuditX.Services.Interfaces
{
    public interface IBranchDepartmentService
    {
        Task<IReadOnlyList<BranchDepartmentListItemViewModel>> GetAllAsync(
            int companyId,
            int? branchId = null,
            int? departmentId = null);

        Task<IReadOnlyList<BranchListItemViewModel>> GetBranchesForDropdownAsync(int companyId);

        Task<IReadOnlyList<DepartmentListItemViewModel>> GetDepartmentsForDropdownAsync(int companyId);

        Task<BranchDepartmentOperationResult> CreateAsync(
            int companyId,
            BranchDepartmentViewModel model);

        Task<BranchDepartmentOperationResult> DeleteAsync(
            int companyId,
            int branchDepartmentId);
    }
}
