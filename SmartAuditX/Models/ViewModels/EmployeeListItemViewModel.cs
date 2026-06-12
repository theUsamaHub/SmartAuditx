using System.ComponentModel.DataAnnotations;

namespace SmartAuditX.Models.ViewModels
{
    public class EmployeeListItemViewModel
    {
        public int EmployeeId { get; set; }
        public string EmployeeCode { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? BranchName { get; set; }
        public string? DepartmentName { get; set; }
        public string? DesignationName { get; set; }
        public bool IsActive { get; set; }
        public bool IsSystemUser { get; set; }
        public DateTime JoiningDate { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
