using SmartAuditX.ViewModels.CMS;

namespace SmartAuditX.Services.Interfaces.CMS
{
    public interface IHowItWorksService
    {
        Task<IEnumerable<HowItWorksStepVM>> GetAllAsync();
        Task<HowItWorksStepVM?> GetByIdAsync(int id);
        Task<bool> CreateAsync(HowItWorksStepVM model);
        Task<bool> UpdateAsync(HowItWorksStepVM model);
        Task<bool> DeleteAsync(int id);
        Task<bool> ToggleStatusAsync(int id);
    }
}
