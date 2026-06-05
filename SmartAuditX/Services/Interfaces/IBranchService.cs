using SmartAuditX.Models.ViewModels;

namespace SmartAuditX.Services.Interfaces
{
    public interface IBranchService
    {
        Task<IReadOnlyList<BranchListItemViewModel>> GetAllAsync(
            int companyId,
            bool? isActive = null,
            string? search = null);

        Task<BranchViewModel?> GetForEditAsync(int companyId, int branchId);

        Task<BranchOperationResult> CreateAsync(int companyId, BranchViewModel model);

        Task<BranchOperationResult> UpdateAsync(
            int companyId,
            int branchId,
            BranchViewModel model);

        Task<BranchOperationResult> DeleteAsync(int companyId, int branchId);

        Task<BranchOperationResult> ToggleActiveAsync(int companyId, int branchId);
    }
}
