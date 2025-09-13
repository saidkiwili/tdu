using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using tae_app.Models;

namespace tae_app.Pages.Admin.Users
{
    [Authorize(Roles = "SuperAdmin,Admin")]
    public class EditModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public EditModel(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public List<IdentityRole> Roles { get; set; } = new();

        public class InputModel
        {
            public string Id { get; set; } = "";
            [Required, EmailAddress]
            public string Email { get; set; } = "";
            [Required]
            public string FirstName { get; set; } = "";
            [Required]
            public string LastName { get; set; } = "";
            public bool IsActive { get; set; } = true;
            [Required]
            public string SelectedRole { get; set; } = "";
        }

        public async Task<IActionResult> OnGetAsync(string id)
        {
            Roles = await _roleManager.Roles.ToListAsync();
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return RedirectToPage("/Admin/Users/Index");

            Input.Id = user.Id;
            Input.Email = user.Email ?? "";
            Input.FirstName = user.FirstName ?? "";
            Input.LastName = user.LastName ?? "";
            Input.IsActive = user.IsActive;
            var userRoles = (await _userManager.GetRolesAsync(user)).ToList();
            Input.SelectedRole = userRoles.FirstOrDefault() ?? "";

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                Roles = await _roleManager.Roles.ToListAsync();
                return Page();
            }

            var user = await _userManager.FindByIdAsync(Input.Id);
            if (user == null) return RedirectToPage("/Admin/Users/Index");

            user.Email = Input.Email;
            user.UserName = Input.Email;
            user.FirstName = Input.FirstName;
            user.LastName = Input.LastName;
            user.IsActive = Input.IsActive;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                foreach (var err in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, err.Description);
                }
                Roles = await _roleManager.Roles.ToListAsync();
                var userRoles = (await _userManager.GetRolesAsync(user)).ToList();
                Input.SelectedRole = userRoles.FirstOrDefault() ?? "";
                return Page();
            }

            // Update role
            var current = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, current);
            if (!string.IsNullOrEmpty(Input.SelectedRole))
            {
                await _userManager.AddToRoleAsync(user, Input.SelectedRole);
            }

            TempData["SuccessMessage"] = "User updated";
            return RedirectToPage("/Admin/Users/Index");
        }
    }
}
