namespace SmartAuditX.Models.ViewModels
{
    public class DepartmentOperationResult
    {
        public bool Success { get; set; }

        public string? Message { get; set; }

        public DepartmentListItemViewModel? Department { get; set; }

        public static DepartmentOperationResult Ok(
            string message,
            DepartmentListItemViewModel? department = null) =>
            new()
            {
                Success = true,
                Message = message,
                Department = department
            };

        public static DepartmentOperationResult Fail(string message) =>
            new()
            {
                Success = false,
                Message = message
            };
    }
}
