namespace SmartAuditX.Models.ViewModels
{
    public class BranchDepartmentOperationResult
    {
        public bool Success { get; set; }

        public string? Message { get; set; }

        public BranchDepartmentListItemViewModel? BranchDepartment { get; set; }

        public static BranchDepartmentOperationResult Ok(
            string message,
            BranchDepartmentListItemViewModel? branchDepartment = null) =>
            new()
            {
                Success = true,
                Message = message,
                BranchDepartment = branchDepartment
            };

        public static BranchDepartmentOperationResult Fail(string message) =>
            new()
            {
                Success = false,
                Message = message
            };
    }
}
