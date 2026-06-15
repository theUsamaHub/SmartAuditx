using SmartAuditX.Models.CMS.ViewModels.CMS;

namespace SmartAuditX.Services.Interfaces.CMS
{
    public interface IAboutUsService
    {
        Task<IEnumerable<AboutUsVM>> GetAllAsync();
        Task<AboutUsVM?> GetByIdAsync(int id);
        Task<bool> CreateAsync(AboutUsVM model);
        Task<bool> UpdateAsync(AboutUsVM model);
        Task<bool> DeleteAsync(int id);
        Task<bool> ToggleStatusAsync(int id);
    }
}
