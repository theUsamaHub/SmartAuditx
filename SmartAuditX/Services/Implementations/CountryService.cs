//using SmartAuditX.Models.ViewModels;

//public class CountryService : ICountryService
//{
//    public List<CountryOptionViewModel> GetCountries()
//    {
//        return new List<CountryOptionViewModel>
//        {
//            new() { Code = "PK", Name = "Pakistan" },
//            new() { Code = "US", Name = "United States" },
//            new() { Code = "GB", Name = "United Kingdom" },
//            new() { Code = "AE", Name = "United Arab Emirates" },
//            new() { Code = "SA", Name = "Saudi Arabia" },
//            new() { Code = "IN", Name = "India" }
//        };
//    }
//}
using PSC.CSharp.Library.CountryData;
using SmartAuditX.Models.ViewModels;

public class CountryService : ICountryService
{
    private readonly CountryHelper _countryHelper;

    public CountryService()
    {
        _countryHelper = new CountryHelper();
    }

    public List<CountryOptionViewModel> GetCountries()
    {
        var countries = _countryHelper.GetCountryData();

        return countries.Select(c => new CountryOptionViewModel
        {
            Code = c.CountryShortCode,  // Alpha-2 code
            Name = c.CountryName
        }).OrderBy(c => c.Name).ToList();
    }
}