using Microsoft.AspNetCore.Identity;
namespace SmartAuditX.Models
{

    public class ApplicationRole : IdentityRole<int>
    {
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public virtual ICollection<ApplicationUserRole> UserRoles { get; set; }  = new List<ApplicationUserRole>(); //added this to establish the relationship with ApplicationUserRole
    }
}
