namespace SmartAuditX.Models.ViewModels
{
    public class DepartmentListItemViewModel
    {
        public int DepartmentId { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Code { get; set; } = string.Empty;

        public string? Description { get; set; }

        public bool IsActive { get; set; }

        public int BranchLinkCount { get; set; }

        public int EmployeeCount { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}
