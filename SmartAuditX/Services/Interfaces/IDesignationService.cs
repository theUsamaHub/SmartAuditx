using SmartAuditX.Models.ViewModels;

namespace SmartAuditX.Services.Interfaces
{
    public interface IDesignationService
    {
        Task<IReadOnlyList<DesignationListItemViewModel>> GetAllAsync(
            int companyId,
            bool? isActive = null,
            string? search = null);

        Task<DesignationViewModel?> GetForEditAsync(int companyId, int designationId);

        Task<DesignationOperationResult> CreateAsync(int companyId, DesignationViewModel model);

        Task<DesignationOperationResult> UpdateAsync(
            int companyId,
            int designationId,
            DesignationViewModel model);

        Task<DesignationOperationResult> DeleteAsync(int companyId, int designationId);

        Task<DesignationOperationResult> ToggleActiveAsync(int companyId, int designationId);
    }
}
