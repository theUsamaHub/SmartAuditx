using SmartAuditX.Models;
using SmartAuditX.Models.ViewModels;

namespace SmartAuditX.Services.Interfaces
{
    public interface ICompanyContactService
    {
        Task<IReadOnlyList<CompanyContactListItemViewModel>> GetAllAsync(
            int companyId,
            ContactType? contactType = null,
            string? search = null);

        Task<CompanyContactViewModel?> GetForEditAsync(int companyId, int contactId);

        Task<CompanyContactOperationResult> CreateAsync(
            int companyId,
            CompanyContactViewModel model);

        Task<CompanyContactOperationResult> UpdateAsync(
            int companyId,
            int contactId,
            CompanyContactViewModel model);

        Task<CompanyContactOperationResult> DeleteAsync(int companyId, int contactId);

        Task<CompanyContactOperationResult> SetPrimaryAsync(int companyId, int contactId);
    }
}
