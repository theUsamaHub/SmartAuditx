using System.ComponentModel.DataAnnotations;

namespace SmartAuditX.Models.ViewModels
{
    public class CountryOptionViewModel
    {
        [Required]
        public string Code { get; set; } = string.Empty;

        [Required]
        public string Name { get; set; } = string.Empty;
    }
}
