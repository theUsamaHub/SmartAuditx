namespace SmartAuditX.Models.ViewModels
{
    public class CompanyContactOperationResult
    {
        public bool Success { get; set; }

        public string? Message { get; set; }

        public CompanyContactListItemViewModel? Contact { get; set; }

        public static CompanyContactOperationResult Ok(
            string message,
            CompanyContactListItemViewModel? contact = null) =>
            new()
            {
                Success = true,
                Message = message,
                Contact = contact
            };

        public static CompanyContactOperationResult Fail(string message) =>
            new()
            {
                Success = false,
                Message = message
            };
    }
}
