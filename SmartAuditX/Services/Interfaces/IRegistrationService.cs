using SmartAuditX.Models.ViewModels;

namespace SmartAuditX.Services.Interfaces
{
    public interface IRegistrationService
    {
        Task<RegistrationResult> RegisterCompanyOwnerAsync(
            RegisterAccountViewModel account,
            RegisterCompanyViewModel company);
    }
}
