using SmartAuditX.Models.CMS.ViewModels.CMS;

namespace SmartAuditX.Services.Interfaces.CMS
{
    public interface IContactInformationService
    {
        Task<IEnumerable<ContactInformationVM>> GetAllAsync();
        Task<ContactInformationVM?> GetByIdAsync(int id);
        Task<bool> CreateAsync(ContactInformationVM model);
        Task<bool> UpdateAsync(ContactInformationVM model);
        Task<bool> DeleteAsync(int id);
        Task<bool> ToggleStatusAsync(int id);
    }
}
