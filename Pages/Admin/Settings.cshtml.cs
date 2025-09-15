using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using System.ComponentModel.DataAnnotations;
using tae_app.Data;
using Microsoft.AspNetCore.Identity;
using tae_app.Models;
using Microsoft.EntityFrameworkCore;

namespace tae_app.Pages.Admin
{
    [Authorize(Roles = "SuperAdmin,Admin")]
    public class SettingsModel : PageModel
    {
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public SettingsModel(IConfiguration configuration, ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _configuration = configuration;
            _context = context;
            _userManager = userManager;
        }

        [BindProperty]
        public SettingsInputModel Input { get; set; } = new();

        // Current settings state
        public bool NidaServicesEnabled { get; set; } = true;
        public decimal NidaIndividualFee { get; set; } = 150.00M;
        public decimal NidaFamilyFee { get; set; } = 400.00M;
        public int NidaProcessingTime { get; set; } = 14;
        public int NidaMaxApplications { get; set; } = 50;

        public class SettingsInputModel
        {
            // General Settings
            [Display(Name = "Site Name")]
            public string? SiteName { get; set; }

            [Display(Name = "Site Description")]
            public string? SiteDescription { get; set; }

            [Display(Name = "Contact Email")]
            [EmailAddress]
            public string? ContactEmail { get; set; }

            [Display(Name = "Contact Phone")]
            public string? ContactPhone { get; set; }

            [Display(Name = "Maintenance Mode")]
            public bool MaintenanceMode { get; set; }

            // Service Settings
            [Display(Name = "NIDA Services Enabled")]
            public bool NidaServicesEnabled { get; set; }

            [Display(Name = "NIDA Individual Fee")]
            [Range(0, 10000)]
            public decimal NidaIndividualFee { get; set; }

            [Display(Name = "NIDA Family Fee")]
            [Range(0, 10000)]
            public decimal NidaFamilyFee { get; set; }

            [Display(Name = "NIDA Processing Time (Days)")]
            [Range(1, 365)]
            public int NidaProcessingTime { get; set; }

            [Display(Name = "NIDA Max Applications/Day")]
            [Range(1, 1000)]
            public int NidaMaxApplications { get; set; }

            [Display(Name = "Job Portal Enabled")]
            public bool JobPortalEnabled { get; set; } = true;

            [Display(Name = "Events Enabled")]
            public bool EventsEnabled { get; set; } = true;

            // Security Settings
            [Display(Name = "Require Strong Password")]
            public bool RequireStrongPassword { get; set; } = true;

            [Display(Name = "Password Expiration")]
            public bool PasswordExpiration { get; set; } = true;

            [Display(Name = "Session Timeout (minutes)")]
            [Range(5, 480)]
            public int SessionTimeout { get; set; } = 30;

            [Display(Name = "Max Login Attempts")]
            [Range(3, 20)]
            public int MaxLoginAttempts { get; set; } = 5;

            // Notification Settings
            [Display(Name = "Notify New Registration")]
            public bool NotifyNewRegistration { get; set; } = true;

            [Display(Name = "Notify NIDA Application")]
            public bool NotifyNidaApplication { get; set; } = true;

            [Display(Name = "Alert System Errors")]
            public bool AlertSystemErrors { get; set; } = true;

            [Display(Name = "Alert Security Threats")]
            public bool AlertSecurityThreats { get; set; } = true;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            // Load current settings from database
            var settings = await _context.AdminSettings.FirstOrDefaultAsync();

            if (settings == null)
            {
                // Create default settings if none exist
                settings = new AdminSettings();
                _context.AdminSettings.Add(settings);
                await _context.SaveChangesAsync();
            }

            // Initialize input model with current values
            Input = new SettingsInputModel
            {
                SiteName = settings.SiteName,
                SiteDescription = settings.SiteDescription,
                ContactEmail = settings.ContactEmail,
                ContactPhone = settings.ContactPhone,
                MaintenanceMode = settings.MaintenanceMode,
                NidaServicesEnabled = settings.NidaServicesEnabled,
                NidaIndividualFee = settings.NidaIndividualFee,
                NidaFamilyFee = settings.NidaFamilyFee,
                NidaProcessingTime = settings.NidaProcessingTime,
                NidaMaxApplications = settings.NidaMaxApplications,
                JobPortalEnabled = settings.JobPortalEnabled,
                EventsEnabled = settings.EventsEnabled,
                RequireStrongPassword = settings.RequireStrongPassword,
                PasswordExpiration = settings.PasswordExpiration,
                SessionTimeout = settings.SessionTimeout,
                MaxLoginAttempts = settings.MaxLoginAttempts,
                NotifyNewRegistration = settings.NotifyNewRegistration,
                NotifyNidaApplication = settings.NotifyNidaApplication,
                AlertSystemErrors = settings.AlertSystemErrors,
                AlertSecurityThreats = settings.AlertSecurityThreats
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Get current user for audit trail
            var currentUser = await _userManager.GetUserAsync(User);
            var userEmail = currentUser?.Email ?? "System";

            // Load or create settings
            var settings = await _context.AdminSettings.FirstOrDefaultAsync();

            if (settings == null)
            {
                settings = new AdminSettings();
                _context.AdminSettings.Add(settings);
            }

            // Update settings with form values
            settings.SiteName = Input.SiteName ?? settings.SiteName;
            settings.SiteDescription = Input.SiteDescription ?? settings.SiteDescription;
            settings.ContactEmail = Input.ContactEmail ?? settings.ContactEmail;
            settings.ContactPhone = Input.ContactPhone ?? settings.ContactPhone;
            settings.MaintenanceMode = Input.MaintenanceMode;
            settings.NidaServicesEnabled = Input.NidaServicesEnabled;
            settings.NidaIndividualFee = Input.NidaIndividualFee;
            settings.NidaFamilyFee = Input.NidaFamilyFee;
            settings.NidaProcessingTime = Input.NidaProcessingTime;
            settings.NidaMaxApplications = Input.NidaMaxApplications;
            settings.JobPortalEnabled = Input.JobPortalEnabled;
            settings.EventsEnabled = Input.EventsEnabled;
            settings.RequireStrongPassword = Input.RequireStrongPassword;
            settings.PasswordExpiration = Input.PasswordExpiration;
            settings.SessionTimeout = Input.SessionTimeout;
            settings.MaxLoginAttempts = Input.MaxLoginAttempts;
            settings.NotifyNewRegistration = Input.NotifyNewRegistration;
            settings.NotifyNidaApplication = Input.NotifyNidaApplication;
            settings.AlertSystemErrors = Input.AlertSystemErrors;
            settings.AlertSecurityThreats = Input.AlertSecurityThreats;
            settings.LastUpdated = DateTime.UtcNow;
            settings.UpdatedBy = userEmail;

            await _context.SaveChangesAsync();

            TempData["StatusMessage"] = "Settings have been updated successfully.";

            // Refresh the page to show updated state
            return RedirectToPage();
        }
    }
}
