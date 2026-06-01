using SmartAuditX.Models.ViewModels;

namespace SmartAuditX.Services.Interfaces
{
    public interface ICityService
    {
        Task<List<CityViewModel>> GetCitiesByCountryCodeAsync(string countryCode);
        Task<List<CityViewModel>> SearchCitiesAsync(string countryCode, string searchTerm);
    }
}