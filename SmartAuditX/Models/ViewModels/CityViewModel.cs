namespace SmartAuditX.Models.ViewModels
{
    public class CityViewModel
    {
        public string Name { get; set; }
        public string? StateCode { get; set; }
        public string? StateName { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
    }
}