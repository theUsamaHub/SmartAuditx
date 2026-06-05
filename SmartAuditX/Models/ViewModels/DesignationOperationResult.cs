namespace SmartAuditX.Models.ViewModels
{
    public class DesignationOperationResult
    {
        public bool Success { get; set; }

        public string? Message { get; set; }

        public DesignationListItemViewModel? Designation { get; set; }

        public static DesignationOperationResult Ok(
            string message,
            DesignationListItemViewModel? designation = null) =>
            new()
            {
                Success = true,
                Message = message,
                Designation = designation
            };

        public static DesignationOperationResult Fail(string message) =>
            new()
            {
                Success = false,
                Message = message
            };
    }
}
