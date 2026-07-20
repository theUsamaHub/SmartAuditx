// Quick test: Run with `dotnet script test-email.csx`
// Or just verify SMTP connectivity

using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

var email = new MimeMessage();
email.From.Add(new MailboxAddress("SmartAuditX", "u641332@gmail.com"));
email.To.Add(MailboxAddress.Parse("u641332@gmail.com"));
email.Subject = "SmartAuditX - Password Reset Test";
email.Body = new BodyBuilder
{
    HtmlBody = "<h2>Test Email</h2><p>If you received this, the email service is working.</p>"
}.ToMessageBody();

using var smtp = new SmtpClient();
try
{
    await smtp.ConnectAsync("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
    Console.WriteLine("[OK] Connected to SMTP server");

    await smtp.AuthenticateAsync("u641332@gmail.com", "exsg thvc xden brpk");
    Console.WriteLine("[OK] Authenticated successfully");

    await smtp.SendAsync(email);
    Console.WriteLine("[OK] Email sent successfully");

    await smtp.DisconnectAsync(true);
    Console.WriteLine("[OK] Disconnected");
}
catch (Exception ex)
{
    Console.WriteLine($"[FAIL] {ex.Message}");
}
