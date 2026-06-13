using SmartAuditX.ViewModels.CMS;

namespace SmartAuditX.Services.Interfaces.CMS
{
    public interface ITeamMemberService
    {
        Task<IEnumerable<TeamMemberVM>> GetAllAsync();
        Task<TeamMemberVM?> GetByIdAsync(int id);
        Task<bool> CreateAsync(TeamMemberVM model);
        Task<bool> UpdateAsync(TeamMemberVM model);
        Task<bool> DeleteAsync(int id);
        Task<bool> ToggleStatusAsync(int id);
    }
}
