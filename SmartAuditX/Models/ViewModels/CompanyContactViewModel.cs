using Humanizer;
using Microsoft.AspNetCore.Mvc.Rendering;
using SmartAuditX.Extensions;
using System.ComponentModel.DataAnnotations;

namespace SmartAuditX.Models.ViewModels
{
    public class CompanyContactViewModel
    {
        public int? CompanyContactId { get; set; }

        [Required(ErrorMessage = "Contact type is required.")]
        public ContactType ContactType { get; set; } = ContactType.HeadOffice;

        [MaxLength(150)]
        [Display(Name = "Contact Name")]
        public string? ContactName { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Enter a valid email address.")]
        [MaxLength(255)]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Dial code is required.")]
        [MaxLength(5)]
        [Display(Name = "Dial Code")]
        public string PhoneDialCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone number is required.")]
        [MaxLength(20)]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; } = string.Empty;

        [MaxLength(20)]
        [Display(Name = "Fax Number")]
        public string? FaxNumber { get; set; }

        [MaxLength(500)]
        [Display(Name = "Physical Address")]
        public string? PhysicalAddress { get; set; }

        [Display(Name = "Primary Contact")]
        public bool IsPrimary { get; set; }

        public List<SelectListItem> ContactTypeOptions =>
            Enum.GetValues<ContactType>()
                .Select(type => new SelectListItem
                {
                    Value = type.ToString(),
                    Text = type.ToString().Humanize()
                })
                .ToList();
    }
}
