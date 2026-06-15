using SmartAuditX.Models.CMS.ViewModels.CMS;

namespace SmartAuditX.Services.Interfaces.CMS
{
    public interface IPlatformModuleService
    {
        Task<IEnumerable<PlatformModuleVM>> GetAllAsync();
        Task<PlatformModuleVM?> GetByIdAsync(int id);
        Task<bool> CreateAsync(PlatformModuleVM model);
        Task<bool> UpdateAsync(PlatformModuleVM model);
        Task<bool> DeleteAsync(int id);
        Task<bool> ToggleStatusAsync(int id);
    }
}
