using SmartAuditX.Models.ViewModels;

namespace SmartAuditX.Services.Interfaces
{
    public interface IRegistrationService
    {
        Task<bool> RegisterCompanyOwnerAsync(
            RegisterAccountViewModel account,
            RegisterCompanyViewModel company);
    }
}
