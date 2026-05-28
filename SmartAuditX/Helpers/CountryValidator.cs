using System.Globalization;

namespace SmartAuditX.Helpers
{
    public static class CountryValidator
    {
        public static bool IsValidCountryCode(
            string countryCode)
        {
            return CultureInfo
                .GetCultures(CultureTypes.SpecificCultures)
                .Select(c =>
                    new RegionInfo(c.Name).TwoLetterISORegionName)
                .Distinct()
                .Contains(countryCode.ToUpper());
        }
    }
}