namespace SmartAuditX.Models.ViewModels
{
    public class BranchListItemViewModel
    {
        public int BranchId { get; set; }

        public string BranchName { get; set; } = string.Empty;

        public string BranchCode { get; set; } = string.Empty;

        public string? Email { get; set; }

        public string? PhoneNumber { get; set; }

        public string? PhysicalAddress { get; set; }

        public bool IsHeadOffice { get; set; }

        public bool IsActive { get; set; }

        public int DepartmentCount { get; set; }

        public int EmployeeCount { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}
