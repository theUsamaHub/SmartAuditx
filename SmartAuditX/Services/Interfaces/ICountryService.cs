using SmartAuditX.Models.ViewModels;

public interface ICountryService
{
    List<CountryOptionViewModel> GetCountries();
}