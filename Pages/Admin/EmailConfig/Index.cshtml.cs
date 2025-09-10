using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Mail;
using System.Text.Json;
using tae_app.Data;
using tae_app.Models;

namespace tae_app.Pages.Admin.EmailConfig
{
    [Authorize(Roles = "SuperAdmin,Admin")]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ApplicationDbContext context, ILogger<IndexModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        [BindProperty]
        public EmailConfigurationModel EmailConfig { get; set; } = new();

        public bool IsEmailConfigured { get; set; }

        public class EmailConfigurationModel
        {
            [Required]
            public string SmtpServer { get; set; } = "";

            [Required]
            [Range(1, 65535)]
            public int SmtpPort { get; set; } = 587;

            [Required]
            public string Username { get; set; } = "";

            [Required]
            public string Password { get; set; } = "";

            [Required]
            [EmailAddress]
            public string FromAddress { get; set; } = "";

            public string FromName { get; set; } = "";

            public bool UseSsl { get; set; } = true;
        }

        public async Task OnGetAsync()
        {
            await LoadEmailConfigAsync();
        }

        public async Task<IActionResult> OnPostSaveConfigAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                // Check if email setting exists
                var emailSetting = await _context.EmailSettings.FirstOrDefaultAsync();
                if (emailSetting == null)
                {
                    emailSetting = new EmailSetting();
                    _context.EmailSettings.Add(emailSetting);
                }

                // Update email settings
                emailSetting.SmtpServer = EmailConfig.SmtpServer;
                emailSetting.SmtpPort = EmailConfig.SmtpPort;
                emailSetting.Username = EmailConfig.Username;
                emailSetting.Password = EmailConfig.Password; // In production, encrypt this
                emailSetting.FromAddress = EmailConfig.FromAddress;
                emailSetting.FromName = EmailConfig.FromName;
                emailSetting.UseSsl = EmailConfig.UseSsl;
                emailSetting.IsActive = true;
                emailSetting.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Email configuration saved successfully!";
                return RedirectToPage();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving email configuration");
                TempData["ErrorMessage"] = "Failed to save email configuration. Please try again.";
                return Page();
            }
        }

        public async Task<IActionResult> OnPostTestEmailAsync([FromBody] TestEmailRequest request)
        {
            try
            {
                var emailSetting = await _context.EmailSettings.FirstOrDefaultAsync();
                if (emailSetting == null)
                {
                    return BadRequest("Email configuration not found");
                }

                using var client = new SmtpClient(emailSetting.SmtpServer, emailSetting.SmtpPort)
                {
                    EnableSsl = emailSetting.UseSsl,
                    Credentials = new NetworkCredential(emailSetting.Username, emailSetting.Password)
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(emailSetting.FromAddress, emailSetting.FromName),
                    Subject = "TAE - Email Configuration Test",
                    Body = @"
                        <h2>Email Configuration Test</h2>
                        <p>This is a test email from the TAE (Tanzanian Association in Emirates) system.</p>
                        <p>If you received this email, your email configuration is working correctly!</p>
                        <hr>
                        <p><small>Sent at: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + @"</small></p>
                    ",
                    IsBodyHtml = true
                };

                mailMessage.To.Add(request.TestEmail);

                await client.SendMailAsync(mailMessage);

                return new JsonResult(new { success = true, message = "Test email sent successfully!" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending test email");
                return BadRequest($"Failed to send test email: {ex.Message}");
            }
        }

        private async Task LoadEmailConfigAsync()
        {
            var emailSetting = await _context.EmailSettings.FirstOrDefaultAsync();
            if (emailSetting != null)
            {
                EmailConfig = new EmailConfigurationModel
                {
                    SmtpServer = emailSetting.SmtpServer ?? "",
                    SmtpPort = emailSetting.SmtpPort,
                    Username = emailSetting.Username ?? "",
                    Password = emailSetting.Password ?? "", // In production, don't expose this
                    FromAddress = emailSetting.FromAddress ?? "",
                    FromName = emailSetting.FromName ?? "",
                    UseSsl = emailSetting.UseSsl
                };
                IsEmailConfigured = emailSetting.IsActive;
            }
            else
            {
                IsEmailConfigured = false;
                EmailConfig = new EmailConfigurationModel
                {
                    SmtpPort = 587,
                    UseSsl = true,
                    FromName = "TAE - Tanzanian Association Emirates"
                };
            }
        }

        public class TestEmailRequest
        {
            public string TestEmail { get; set; } = "";
        }
    }
}
