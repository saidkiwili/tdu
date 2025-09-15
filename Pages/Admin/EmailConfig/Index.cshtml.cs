using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Mail;
using tae_app.Data;
using tae_app.Models;

namespace tae_app.Pages.Admin.EmailConfig
{
    [Authorize(Roles = "SuperAdmin,Admin")]
    [ValidateAntiForgeryToken]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<IndexModel> _logger;
        private readonly IAuthorizationService _authorizationService;

        public IndexModel(ApplicationDbContext context, ILogger<IndexModel> logger, IAuthorizationService authorizationService)
        {
            _context = context;
            _logger = logger;
            _authorizationService = authorizationService;
        }

        [BindProperty]
        public EmailConfigurationModel EmailConfig { get; set; } = new();

        public bool IsEmailConfigured { get; set; }
    // UI permission flags
    public bool CanEdit { get; set; }

        public class EmailConfigurationModel
        {
            [Required]
            public string SmtpServer { get; set; } = "";

            [Required]
            [Range(1, 65535)]
            public int SmtpPort { get; set; } = 587;

            [Required]
            public string Username { get; set; } = "";

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

            // evaluate permission for editing email settings
            CanEdit = (await _authorizationService.AuthorizeAsync(User, "permission:settings.edit")).Succeeded;
        }

        public async Task<IActionResult> OnPostSaveConfigAsync()
        {
            if (!(await _authorizationService.AuthorizeAsync(User, "permission:settings.edit")).Succeeded)
            {
                return Forbid();
            }

            // Custom validation: Password is required only for new configurations
            var existingEmailSetting = await _context.EmailSettings.FirstOrDefaultAsync();
            if (existingEmailSetting == null && string.IsNullOrEmpty(EmailConfig.Password))
            {
                ModelState.AddModelError("EmailConfig.Password", "Password is required when setting up email configuration for the first time.");
            }

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
                // Only update password if a new one is provided
                if (!string.IsNullOrEmpty(EmailConfig.Password))
                {
                    emailSetting.Password = EmailConfig.Password; // In production, encrypt this
                }
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

        public async Task<IActionResult> OnPostTestEmailAsync()
        {
            _logger.LogInformation("TestEmail handler called - using database configuration only");

            // Check if user has admin role instead of specific permission
            if (!User.IsInRole("Admin") && !User.IsInRole("SuperAdmin"))
            {
                _logger.LogWarning("User {User} does not have admin role", User?.Identity?.Name);
                return new JsonResult(new { success = false, message = "You don't have permission to test email configuration." });
            }

            try
            {
                // Load email configuration from database only
                var emailSetting = await _context.EmailSettings.FirstOrDefaultAsync();
                if (emailSetting == null)
                {
                    _logger.LogWarning("Email configuration not found in database");
                    return new JsonResult(new { success = false, message = "Email configuration not found. Please configure email settings first." });
                }

                // Validate required fields
                if (string.IsNullOrEmpty(emailSetting.SmtpServer) ||
                    string.IsNullOrEmpty(emailSetting.Username) ||
                    string.IsNullOrEmpty(emailSetting.Password) ||
                    string.IsNullOrEmpty(emailSetting.FromAddress))
                {
                    _logger.LogWarning("Email configuration is incomplete - missing required fields");
                    return new JsonResult(new { success = false, message = "Email configuration is incomplete. Please ensure all required fields (SMTP Server, Username, Password, From Address) are configured." });
                }

             

                // Use a default test email (could be made configurable in the future)
                var testEmail = "saidkiwilii@gmail.com";

               

                using var client = new SmtpClient(emailSetting.SmtpServer, emailSetting.SmtpPort)
                {
                    EnableSsl = emailSetting.UseSsl,
                    Credentials = new NetworkCredential(emailSetting.Username, emailSetting.Password),
                    Timeout = 30000 // 30 second timeout
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(emailSetting.FromAddress, emailSetting.FromName),
                    Subject = "TDUAE - Email Configuration Test",
                    Body = $@"
                        <h2>Email Configuration Test</h2>
                        <p>This is a test email from the TDUAE (Tanzania Diaspora UAE) system.</p>
                        <p><strong>Configuration Details:</strong></p>
                        <ul>
                            <li>SMTP Server: {emailSetting.SmtpServer}:{emailSetting.SmtpPort}</li>
                            <li>SSL/TLS: {(emailSetting.UseSsl ? "Enabled" : "Disabled")}</li>
                            <li>From: {emailSetting.FromAddress}</li>
                        </ul>
                        <p>If you received this email, your email configuration is working correctly!</p>
                        <hr>
                        <p><small>Test sent at: {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}</small></p>
                    ",
                    IsBodyHtml = true
                };

                mailMessage.To.Add(testEmail);

                await client.SendMailAsync(mailMessage);

                _logger.LogInformation("Test email sent successfully to {Email} using database configuration", testEmail);
                return new JsonResult(new { success = true, message = $"Test email sent successfully to {testEmail} using saved configuration!" });
            }
            catch (System.Net.Mail.SmtpException smtpEx)
            {
                _logger.LogError(smtpEx, "SMTP error sending test email: {Message}", smtpEx.Message);

                string errorMessage = "SMTP Authentication failed. Please check your email configuration:\n";
                errorMessage += "• Verify your username and password are correct\n";
                errorMessage += "• Check if your email provider requires an app password instead of your regular password\n";
                errorMessage += "• Ensure SSL/TLS settings match your email provider's requirements\n";
                errorMessage += $"• SMTP Error: {smtpEx.Message}";

                return new JsonResult(new { success = false, message = errorMessage });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending test email using database configuration: {Message}", ex.Message);

                string errorMessage = $"Failed to send test email: {ex.Message}";
                if (ex.InnerException != null)
                {
                    errorMessage += $" (Inner: {ex.InnerException.Message})";
                }

                return new JsonResult(new { success = false, message = errorMessage });
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
                    Password = "", // Don't expose existing password for security
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
                    FromName = "TDUAE"
                };
            }
        }


    }
}
