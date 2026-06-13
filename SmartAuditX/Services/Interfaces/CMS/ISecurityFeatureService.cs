using SmartAuditX.ViewModels.CMS;

namespace SmartAuditX.Services.Interfaces.CMS
{
    public interface ISecurityFeatureService
    {
        Task<IEnumerable<SecurityFeatureVM>> GetAllAsync();
        Task<SecurityFeatureVM?> GetByIdAsync(int id);
        Task<bool> CreateAsync(SecurityFeatureVM model);
        Task<bool> UpdateAsync(SecurityFeatureVM model);
        Task<bool> DeleteAsync(int id);
        Task<bool> ToggleStatusAsync(int id);
    }
}
