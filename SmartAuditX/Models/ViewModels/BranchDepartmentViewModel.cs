using System.ComponentModel.DataAnnotations;

namespace SmartAuditX.Models.ViewModels
{
    public class BranchDepartmentViewModel
    {
        public int? BranchDepartmentId { get; set; }

        [Required(ErrorMessage = "Branch is required.")]
        [Display(Name = "Branch")]
        public int BranchId { get; set; }

        [Required(ErrorMessage = "Department is required.")]
        [Display(Name = "Department")]
        public int DepartmentId { get; set; }
    }
}
