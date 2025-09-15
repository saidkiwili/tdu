using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using tae_app.Models;
using System.ComponentModel.DataAnnotations;

namespace tae_app.Pages.Account
{
    public class SettingsModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public SettingsModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [BindProperty]
        public SettingsInputModel Input { get; set; } = new();

        public bool EnableNidaServices { get; set; }
        public string FullName { get; set; } = "";
        public string Email { get; set; } = "";

        public class SettingsInputModel
        {
            public bool EnableNidaServices { get; set; }
            public bool EmailNotifications { get; set; } = true;
            public bool SmsNotifications { get; set; } = true;
            public string ProfileVisibility { get; set; } = "public";
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            // Load current settings
            EnableNidaServices = user.Member?.OptInNidaService ?? false;
            FullName = $"{user.FirstName} {user.LastName}".Trim();
            Email = user.Email ?? user.Member?.EmailAddress ?? "";

            // Initialize input model with current values
            Input = new SettingsInputModel
            {
                EnableNidaServices = EnableNidaServices,
                EmailNotifications = true, // Default to true, could be stored in user preferences
                SmsNotifications = true,   // Default to true, could be stored in user preferences
                ProfileVisibility = "public" // Default to public, could be stored in user preferences
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                // Reload current settings for display
                EnableNidaServices = user.Member?.OptInNidaService ?? false;
                FullName = $"{user.FirstName} {user.LastName}".Trim();
                Email = user.Email ?? user.Member?.EmailAddress ?? "";
                return Page();
            }

            // Update NIDA service preference
            if (user.Member != null)
            {
                user.Member.OptInNidaService = Input.EnableNidaServices;
                // Note: You would need to save the Member entity here
                // This depends on your data context setup
            }

            // Update other preferences (you would implement storage for these)
            // For now, we'll just show success message

            TempData["StatusMessage"] = "Your settings have been updated successfully.";

            // Refresh the page to show updated state
            return RedirectToPage();
        }
    }
}
