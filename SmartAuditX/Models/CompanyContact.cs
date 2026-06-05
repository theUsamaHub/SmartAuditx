using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartAuditX.Models
{
    /// <summary>
    /// Contact type enum — replaces using ContactName as both a label and type.
    /// Enables proper filtering: "give me all Finance contacts across branches".
    /// </summary>
    public enum ContactType
    {
        HeadOffice,
        HR,
        Finance,
        Legal,
        Operations,      
              // ── Add these ─────────────────────
    IT,               // Technical support, software issues
        Sales,            // Sales inquiries and deals
        CustomerSupport,  // Client-facing support desk
        Procurement,      // Purchasing and vendor management
        Logistics,        // Warehouse, delivery, supply chain
        Management,       // C-level / executive contacts
        Compliance,       // Audit, regulatory, risk management
        Admin,            // General administration

        Other
    }

    public class CompanyContact
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CompanyContactId { get; set; }

        [Required]
        [ForeignKey("Company")]
        public int CompanyId { get; set; }

        /// <summary>
        /// What type of contact this is.
        /// Replaces ContactName doing double duty as both a label and a type.
        /// Now you can filter by type reliably.
        /// </summary>
        [Required]
        [Column(TypeName = "nvarchar(30)")]
        public ContactType ContactType { get; set; } = ContactType.HeadOffice;

        /// <summary>Actual person's name at this contact point. e.g. "Ali Hassan".</summary>
        [MaxLength(150)]
        public string? ContactName { get; set; }

        [Required]
        [MaxLength(255)]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MaxLength(5)]
        public string PhoneDialCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string PhoneNumber { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? FaxNumber { get; set; }

        [MaxLength(500)]
        public string? PhysicalAddress { get; set; }

        public bool IsPrimary { get; set; } = false;

        public bool IsDeleted { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation
        public virtual Company? Company { get; set; }
    }
}