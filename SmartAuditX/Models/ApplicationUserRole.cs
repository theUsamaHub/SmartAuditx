using Microsoft.AspNetCore.Identity;

namespace SmartAuditX.Models
{
    public class ApplicationUserRole : IdentityUserRole<int>
    {
        public int UserRoleId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public virtual ApplicationUser User { get; set; }

        public virtual ApplicationRole Role { get; set; }
    }
}
