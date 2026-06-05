namespace SmartAuditX.Models.ViewModels
{
    public class BranchDepartmentListItemViewModel
    {
        public int BranchDepartmentId { get; set; }

        public int BranchId { get; set; }

        public string BranchName { get; set; } = string.Empty;

        public string BranchCode { get; set; } = string.Empty;

        public int DepartmentId { get; set; }

        public string DepartmentName { get; set; } = string.Empty;

        public string DepartmentCode { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
    }
}
