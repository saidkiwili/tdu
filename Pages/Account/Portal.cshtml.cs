using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using tae_app.Models;

namespace tae_app.Pages.Account
{
    public class PortalModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public PortalModel(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public string FullName { get; set; } = "";
        public IList<string> Roles { get; set; } = new List<string>();

        public async Task OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                FullName = $"{user.FirstName} {user.LastName}".Trim();
                Roles = (await _userManager.GetRolesAsync(user)).ToList();
            }
        }
    }
}
