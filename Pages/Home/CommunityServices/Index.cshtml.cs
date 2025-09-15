using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using tae_app.Models;
using tae_app.Services;

namespace tae_app.Pages.Home.CommunityServices
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly AdminSettingsService _adminSettingsService;

        public IndexModel(UserManager<ApplicationUser> userManager, AdminSettingsService adminSettingsService)
        {
            _userManager = userManager;
            _adminSettingsService = adminSettingsService;
        }

        public bool ShowNidaServices { get; set; } = false;

        public async Task OnGetAsync()
        {
            // Check if NIDA services are globally enabled by admin
            bool nidaGloballyEnabled = await _adminSettingsService.IsNidaServicesEnabledAsync();

            if (!nidaGloballyEnabled)
            {
                ShowNidaServices = false;
                return;
            }

            // Check if user is authenticated and has opted in
            if (User.Identity?.IsAuthenticated == true)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user?.Member != null)
                {
                    // Show NIDA services if user has opted in
                    ShowNidaServices = user.Member.OptInNidaService;
                }
                else
                {
                    // New user, default to showing NIDA services
                    ShowNidaServices = true;
                }
            }
            else
            {
                // For non-authenticated users, show NIDA services by default
                ShowNidaServices = true;
            }
        }
    }
}
