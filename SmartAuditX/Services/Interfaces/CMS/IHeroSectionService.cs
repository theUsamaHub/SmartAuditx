using SmartAuditX.Models.CMS.ViewModels.CMS;

namespace SmartAuditX.Services.Interfaces.CMS
{
    public interface IHeroSectionService
    {
        Task<IEnumerable<HeroSectionVM>> GetAllAsync();
        Task<HeroSectionVM?> GetByIdAsync(int id);
        Task<bool> CreateAsync(HeroSectionVM model);
        Task<bool> UpdateAsync(HeroSectionVM model);
        Task<bool> DeleteAsync(int id);
        Task<bool> ToggleStatusAsync(int id);
    }
}
