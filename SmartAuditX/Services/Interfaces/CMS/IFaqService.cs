using SmartAuditX.ViewModels.CMS;

namespace SmartAuditX.Services.Interfaces.CMS
{
    public interface IFaqService
    {
        Task<IEnumerable<FaqVM>> GetAllAsync();
        Task<FaqVM?> GetByIdAsync(int id);
        Task<bool> CreateAsync(FaqVM model);
        Task<bool> UpdateAsync(FaqVM model);
        Task<bool> DeleteAsync(int id);
        Task<bool> ToggleStatusAsync(int id);
    }
}
