using SmartAuditX.Models.ViewModels;
using SmartAuditX.Services.Interfaces;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SmartAuditX.Services.Implementations
{
    public class CityService : ICityService
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions;

        public CityService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        public async Task<List<CityViewModel>> GetCitiesByCountryCodeAsync(string countryCode)
        {
            try
            {
                // Using GeoDB Cities API (Free tier: 500 requests/day)
                // Sign up at https://rapidapi.com/wirefreethought/api/geodb-cities/
                // Or use the free OpenDataSoft API below

                var url = $"https://public.opendatasoft.com/api/explore/v2.1/catalog/datasets/geonames-all-cities-with-a-population-1000/records?where=country_code%3D%22{countryCode}%22&limit=100&order_by=population%20DESC";

                var response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<OpenDataSoftResponse>(json, _jsonOptions);

                    return result?.Records?.Select(r => new CityViewModel
                    {
                        Name = r.Record.Fields.Name,
                        Latitude = r.Record.Fields.Coordinates?.Lat,
                        Longitude = r.Record.Fields.Coordinates?.Lon,
                        StateCode = r.Record.Fields.Admin1_Code,
                        StateName = r.Record.Fields.Admin1_Name
                    }).ToList() ?? new List<CityViewModel>();
                }

                // Fallback to local cache if API fails
                return GetCachedCities(countryCode);
            }
            catch
            {
                // Return cached cities on error
                return GetCachedCities(countryCode);
            }
        }

        public async Task<List<CityViewModel>> SearchCitiesAsync(string countryCode, string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm) || searchTerm.Length < 2)
                return new List<CityViewModel>();

            try
            {
                var url = $"https://public.opendatasoft.com/api/explore/v2.1/catalog/datasets/geonames-all-cities-with-a-population-1000/records?where=country_code%3D%22{countryCode}%22%20AND%20name%20LIKE%20%22%25{Uri.EscapeDataString(searchTerm)}%25%22&limit=20";

                var response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<OpenDataSoftResponse>(json, _jsonOptions);

                    return result?.Records?.Select(r => new CityViewModel
                    {
                        Name = r.Record.Fields.Name,
                        Latitude = r.Record.Fields.Coordinates?.Lat,
                        Longitude = r.Record.Fields.Coordinates?.Lon,
                        StateCode = r.Record.Fields.Admin1_Code,
                        StateName = r.Record.Fields.Admin1_Name
                    }).ToList() ?? new List<CityViewModel>();
                }

                return GetCachedCities(countryCode)
                    .Where(c => c.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                    .Take(20)
                    .ToList();
            }
            catch
            {
                return GetCachedCities(countryCode)
                    .Where(c => c.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                    .Take(20)
                    .ToList();
            }
        }

        // Fallback: Static major cities per country
        private List<CityViewModel> GetCachedCities(string countryCode)
        {
            return countryCode.ToUpper() switch
            {
                "PK" => new List<CityViewModel>
                {
                    new() { Name = "Karachi", StateName = "Sindh" },
                    new() { Name = "Lahore", StateName = "Punjab" },
                    new() { Name = "Islamabad", StateName = "Islamabad Capital Territory" },
                    new() { Name = "Rawalpindi", StateName = "Punjab" },
                    new() { Name = "Faisalabad", StateName = "Punjab" },
                    new() { Name = "Multan", StateName = "Punjab" },
                    new() { Name = "Gujranwala", StateName = "Punjab" },
                    new() { Name = "Peshawar", StateName = "Khyber Pakhtunkhwa" },
                    new() { Name = "Quetta", StateName = "Balochistan" },
                    new() { Name = "Sialkot", StateName = "Punjab" }
                },
                "US" => new List<CityViewModel>
                {
                    new() { Name = "New York", StateName = "New York" },
                    new() { Name = "Los Angeles", StateName = "California" },
                    new() { Name = "Chicago", StateName = "Illinois" },
                    new() { Name = "Houston", StateName = "Texas" },
                    new() { Name = "Phoenix", StateName = "Arizona" },
                    new() { Name = "Philadelphia", StateName = "Pennsylvania" },
                    new() { Name = "San Antonio", StateName = "Texas" },
                    new() { Name = "San Diego", StateName = "California" },
                    new() { Name = "Dallas", StateName = "Texas" },
                    new() { Name = "Austin", StateName = "Texas" }
                },
                "GB" => new List<CityViewModel>
                {
                    new() { Name = "London" },
                    new() { Name = "Birmingham" },
                    new() { Name = "Manchester" },
                    new() { Name = "Glasgow" },
                    new() { Name = "Liverpool" },
                    new() { Name = "Bristol" },
                    new() { Name = "Sheffield" },
                    new() { Name = "Leeds" },
                    new() { Name = "Edinburgh" },
                    new() { Name = "Leicester" }
                },
                "AE" => new List<CityViewModel>
                {
                    new() { Name = "Dubai" },
                    new() { Name = "Abu Dhabi" },
                    new() { Name = "Sharjah" },
                    new() { Name = "Ajman" },
                    new() { Name = "Ras Al Khaimah" },
                    new() { Name = "Fujairah" },
                    new() { Name = "Umm Al Quwain" }
                },
                "SA" => new List<CityViewModel>
                {
                    new() { Name = "Riyadh" },
                    new() { Name = "Jeddah" },
                    new() { Name = "Mecca" },
                    new() { Name = "Medina" },
                    new() { Name = "Dammam" }
                },
                "IN" => new List<CityViewModel>
                {
                    new() { Name = "Mumbai", StateName = "Maharashtra" },
                    new() { Name = "Delhi", StateName = "Delhi" },
                    new() { Name = "Bangalore", StateName = "Karnataka" },
                    new() { Name = "Hyderabad", StateName = "Telangana" },
                    new() { Name = "Ahmedabad", StateName = "Gujarat" },
                    new() { Name = "Chennai", StateName = "Tamil Nadu" },
                    new() { Name = "Kolkata", StateName = "West Bengal" },
                    new() { Name = "Pune", StateName = "Maharashtra" }
                },
                _ => new List<CityViewModel>
                {
                    new() { Name = "Select a country first" }
                }
            };
        }
    }

    // API Response Models
    public class OpenDataSoftResponse
    {
        public List<RecordWrapper> Records { get; set; }
    }

    public class RecordWrapper
    {
        public CityRecord Record { get; set; }
    }

    public class CityRecord
    {
        public CityFields Fields { get; set; }
    }

    public class CityFields
    {
        public string Name { get; set; }
        public string Country_Code { get; set; }
        public string Admin1_Code { get; set; }
        public string Admin1_Name { get; set; }
        public Coordinates Coordinates { get; set; }
    }

    public class Coordinates
    {
        [JsonPropertyName("lat")]
        public double Lat { get; set; }

        [JsonPropertyName("lon")]
        public double Lon { get; set; }
    }
}