using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace SmartAuditX.Models
{
    /// <summary>
    /// Extends ASP.NET Identity's IdentityUser with OpsPulse-specific fields.
    /// Created alongside Company in a single atomic transaction during onboarding Step 1.
    /// This is the admin user of the company at registration time.
    /// Additional users are created later from within the platform.
    /// 
    /// IMPORTANT: Never modify IdentityUser directly.
    /// All custom fields go here in ApplicationUser.
    /// </summary>
    public class ApplicationUser : IdentityUser<int>
    {
        // ── Company & Employee Link ────────────────────────────────────────────

        /// <summary>
        /// Every user belongs to exactly one Company (tenant isolation).
        /// Set during registration and never changes.
        /// </summary>
        [ForeignKey("Company")]
        public int CompanyId { get; set; }

        /// <summary>
        /// Links the system user account to an Employee record if applicable.
        /// Nullable — the initial admin user at registration has no employee record yet.
        /// Set later when the admin creates their own employee profile.
        /// </summary>
        [ForeignKey("Employee")]
        public int? EmployeeId { get; set; }


        // ── Phone Dial Code ───────────────────────────────────────────────────

        /// <summary>
        /// International dialing code for the user's phone number.
        /// e.g. "+92" for Pakistan, "+1" for USA, "+971" for UAE.
        /// 
        /// Stored SEPARATELY from IdentityUser.PhoneNumber intentionally:
        /// - IdentityUser.PhoneNumber stores the LOCAL number only (e.g. "3001234567")
        /// - This field stores the dial code (e.g. "+92")
        /// - Full number = PhoneDialCode + PhoneNumber = "+923001234567"
        /// 
        /// Why separate? SMS providers (Twilio, etc.) expect them split.
        /// Also prevents mismatch — user picks USA (+1) but types a Pakistani number.
        /// Frontend auto-fills this from the Country selection but keeps it editable.
        /// </summary>
        /// 

        [MaxLength(5)]
        [Required]
        public string PhoneDialCode { get; set; }

        /// <summary>
        /// Full international phone number for display and SMS sending.
        /// Not mapped to DB — computed from PhoneDialCode + PhoneNumber.
        /// </summary>
        ///

        [NotMapped]
        public string? FullPhoneNumber =>
            string.IsNullOrEmpty(PhoneDialCode)
                ? PhoneNumber
                : $"{PhoneDialCode}{PhoneNumber}";

        // ── Soft Delete & Timestamps ──────────────────────────────────────────

        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }


        // ── Navigation Properties ─────────────────────────────────────────────

        public virtual Company? Company { get; set; }
        public virtual Employee? Employee { get; set; }

        /// <summary>
        /// Many-to-many with roles via ApplicationUserRole join table.
        /// </summary>
        public virtual ICollection<ApplicationUserRole> UserRoles { get; set; } = new List<ApplicationUserRole>();
    }
}