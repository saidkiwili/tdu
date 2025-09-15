using System.Net;
using System.Net.Mail;
using Microsoft.EntityFrameworkCore;
using tae_app.Data;

namespace tae_app.Services;

public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string body);
}

public class EmailService : IEmailService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<EmailService> _logger;

    public EmailService(ApplicationDbContext context, ILogger<EmailService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task SendEmailAsync(string to, string subject, string body)
    {
        try
        {
            // Get email settings from database
            var emailSetting = await _context.EmailSettings
                .FirstOrDefaultAsync(es => es.IsActive);

            if (emailSetting == null || string.IsNullOrEmpty(emailSetting.Username) || string.IsNullOrEmpty(emailSetting.Password))
            {
                _logger.LogWarning("No active email settings found in database or credentials are missing. Email sending disabled.");
                return;
            }

            using (var client = new SmtpClient(emailSetting.SmtpServer, emailSetting.SmtpPort))
            {
                client.Credentials = new NetworkCredential(emailSetting.Username, emailSetting.Password);
                client.EnableSsl = emailSetting.UseSsl;

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(emailSetting.FromAddress, emailSetting.FromName ?? "TDUAE"),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };

                mailMessage.To.Add(to);

                await client.SendMailAsync(mailMessage);
            }

            _logger.LogInformation("Email sent successfully to: {To}", to);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to: {To}", to);
            // For development/demo purposes, log the email content
            _logger.LogInformation("Email content - To: {To}, Subject: {Subject}", to, subject);
        }
    }
}
