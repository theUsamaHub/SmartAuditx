using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartAuditX.Models
{
    public class BranchDepartment
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int BranchDepartmentId { get; set; }

        [Required]
        public int BranchId { get; set; }

        [Required]
        public int DepartmentId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }
        // Navigation
        //public virtual required Branch Branch { get; set; }

        public virtual Branch Branch { get; set; }

        //public virtual required Department Department { get; set; }

        public virtual Department Department { get; set; }

    }
}