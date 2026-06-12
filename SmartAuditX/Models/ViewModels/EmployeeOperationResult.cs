namespace SmartAuditX.Models.ViewModels
{
    public class EmployeeOperationResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public EmployeeListItemViewModel? Employee { get; set; }

        public static EmployeeOperationResult Ok(string message, EmployeeListItemViewModel? employee = null)
        {
            return new EmployeeOperationResult
            {
                Success = true,
                Message = message,
                Employee = employee
            };
        }

        public static EmployeeOperationResult Fail(string message)
        {
            return new EmployeeOperationResult
            {
                Success = false,
                Message = message
            };
        }
    }
}
