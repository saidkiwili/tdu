using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using tae_app.Models;

namespace tae_app.Pages.Account
{
    public class ProfileModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public ProfileModel(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public string Initials { get; set; } = "";
        public string FullName { get; set; } = "";
        public string Email { get; set; } = "";
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string PhoneNumber { get; set; } = "";
        public DateTime? DateOfBirth { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Status { get; set; } = "Active";
        public IList<string> Roles { get; set; } = new List<string>();

        public async Task OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return;
            }

            FirstName = user.FirstName ?? string.Empty;
            LastName = user.LastName ?? string.Empty;
            FullName = $"{FirstName} {LastName}".Trim();

            // Prefer identity email, fall back to related Member email if present
            Email = user.Email ?? user.Member?.EmailAddress ?? string.Empty;

            // Prefer phone from Identity user, then Member
            PhoneNumber = user.PhoneNumber ?? user.Member?.PhoneNumber ?? string.Empty;

            // DateOfBirth is part of Member, not ApplicationUser
            DateOfBirth = user.Member?.DateOfBirth;

            CreatedAt = user.CreatedAt;
            Initials = ((FirstName?.Length > 0 ? FirstName[0].ToString() : "") + (LastName?.Length > 0 ? LastName[0].ToString() : "")).ToUpper();
            Roles = (await _userManager.GetRolesAsync(user)).ToList();
            Status = user.IsActive ? "Active" : "Inactive";
        }
    }
}
