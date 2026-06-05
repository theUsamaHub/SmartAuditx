namespace SmartAuditX.Models.ViewModels
{
    public class BranchOperationResult
    {
        public bool Success { get; set; }

        public string? Message { get; set; }

        public BranchListItemViewModel? Branch { get; set; }

        public static BranchOperationResult Ok(
            string message,
            BranchListItemViewModel? branch = null) =>
            new()
            {
                Success = true,
                Message = message,
                Branch = branch
            };

        public static BranchOperationResult Fail(string message) =>
            new()
            {
                Success = false,
                Message = message
            };
    }
}
