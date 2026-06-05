namespace SmartAuditX.Models.ViewModels
{
    public class CompanyContactListItemViewModel
    {
        public int CompanyContactId { get; set; }

        public ContactType ContactType { get; set; }

        public string ContactTypeDisplay { get; set; } = string.Empty;

        public string? ContactName { get; set; }

        public string Email { get; set; } = string.Empty;

        public string PhoneDialCode { get; set; } = string.Empty;

        public string PhoneNumber { get; set; } = string.Empty;

        public string FullPhone => $"{PhoneDialCode}{PhoneNumber}";

        public string? FaxNumber { get; set; }

        public string? PhysicalAddress { get; set; }

        public bool IsPrimary { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}
