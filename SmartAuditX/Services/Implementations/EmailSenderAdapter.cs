using Microsoft.AspNetCore.Identity.UI.Services;
using SmartAuditX.Services.Interfaces;

namespace SmartAuditX.Services.Implementations
{
    /// <summary>
    /// Bridges ASP.NET Identity's IEmailSender to the custom IEmailService.
    /// Used by Identity Razor Pages (ForgotPassword, RegisterConfirmation, etc.)
    /// that depend on IEmailSender.
    /// </summary>
    public class EmailSenderAdapter : IEmailSender
    {
        private readonly IEmailService _emailService;

        public EmailSenderAdapter(IEmailService emailService)
        {
            _emailService = emailService;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            await _emailService.SendEmailAsync(email, subject, htmlMessage);
        }
    }
}
