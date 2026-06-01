namespace SmartAuditX.Models.ViewModels
{
    public class RegistrationResult
    {
        public bool Success { get; set; }

        public int? UserId { get; set; } = 0;

        public string? EncodedToken { get; set; } = string.Empty;

        public string? ErrorMessage { get; set; } = string.Empty;
    }
}
