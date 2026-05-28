using SmartAuditX.Models.ViewModels;

public class CountryService : ICountryService
{
    public List<CountryOptionViewModel> GetCountries()
    {
        return new List<CountryOptionViewModel>
        {
            new() { Code = "PK", Name = "Pakistan" },
            new() { Code = "US", Name = "United States" },
            new() { Code = "GB", Name = "United Kingdom" },
            new() { Code = "AE", Name = "United Arab Emirates" },
            new() { Code = "SA", Name = "Saudi Arabia" },
            new() { Code = "IN", Name = "India" }
        };
    }
}