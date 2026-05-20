using Microsoft.AspNetCore.Identity;

namespace SmartAuditX.Models
{
    public class ApplicationUser : IdentityUser<int>
    {
        public int CompanyId { get; set; }

        public int? EmployeeId { get; set; }

        public bool IsActive { get; set; } = true;

        public bool IsDeleted { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public virtual ICollection<ApplicationUserRole> UserRoles { get; set; }  = new List<ApplicationUserRole>(); //added this to establish the relationship with ApplicationUserRole
    }
}
