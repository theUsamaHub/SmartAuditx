using SmartAuditX.Models.CMS.ViewModels.CMS;

namespace SmartAuditX.Services.Interfaces.CMS
{
    public interface IFeatureService
    {
        Task<IEnumerable<FeatureVM>> GetAllAsync();
        Task<FeatureVM?> GetByIdAsync(int id);
        Task<bool> CreateAsync(FeatureVM model);
        Task<bool> UpdateAsync(FeatureVM model);
        Task<bool> DeleteAsync(int id);
        Task<bool> ToggleStatusAsync(int id);
    }
}
