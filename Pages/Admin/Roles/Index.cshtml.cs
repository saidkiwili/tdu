using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using tae_app.Models;

namespace tae_app.Pages.Admin.Roles
{
    [Authorize(Roles = "SuperAdmin,Admin")]
    public class IndexModel : PageModel
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public IndexModel(RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager)
        {
            _roleManager = roleManager;
            _userManager = userManager;
        }

        public List<RoleViewModel> Roles { get; set; } = new();
        public List<string> AvailablePermissions { get; set; } = new();

        [BindProperty]
        public CreateRoleModel CreateRole { get; set; } = new();

        [BindProperty]
        public EditRoleModel EditRole { get; set; } = new();

        public class RoleViewModel
        {
            public string Id { get; set; } = "";
            public string Name { get; set; } = "";
            public int UserCount { get; set; }
            public List<string> Permissions { get; set; } = new();
        }

        public class CreateRoleModel
        {
            [Required]
            public string Name { get; set; } = "";
            public List<string> SelectedPermissions { get; set; } = new();
        }

        public class EditRoleModel
        {
            public string Id { get; set; } = "";
            [Required]
            public string Name { get; set; } = "";
            public List<string> SelectedPermissions { get; set; } = new();
        }

        public async Task OnGetAsync()
        {
            await LoadDataAsync();
        }

        public async Task<IActionResult> OnPostCreateAsync()
        {
            if (!ModelState.IsValid)
            {
                await LoadDataAsync();
                return Page();
            }

            var role = new IdentityRole(CreateRole.Name);
            var result = await _roleManager.CreateAsync(role);

            if (result.Succeeded)
            {
                // Add permissions as claims
                if (CreateRole.SelectedPermissions?.Any() == true)
                {
                    foreach (var permission in CreateRole.SelectedPermissions)
                    {
                        await _roleManager.AddClaimAsync(role, new Claim("permission", permission));
                    }
                }

                TempData["SuccessMessage"] = "Role created successfully!";
                return RedirectToPage();
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            await LoadDataAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostEditAsync()
        {
            if (!ModelState.IsValid)
            {
                await LoadDataAsync();
                return Page();
            }

            var role = await _roleManager.FindByIdAsync(EditRole.Id);
            if (role == null)
            {
                TempData["ErrorMessage"] = "Role not found!";
                return RedirectToPage();
            }

            role.Name = EditRole.Name;
            var result = await _roleManager.UpdateAsync(role);

            if (result.Succeeded)
            {
                // Update permissions
                var existingClaims = await _roleManager.GetClaimsAsync(role);
                var permissionClaims = existingClaims.Where(c => c.Type == "permission").ToList();
                
                // Remove existing permission claims
                foreach (var claim in permissionClaims)
                {
                    await _roleManager.RemoveClaimAsync(role, claim);
                }

                // Add new permission claims
                if (EditRole.SelectedPermissions?.Any() == true)
                {
                    foreach (var permission in EditRole.SelectedPermissions)
                    {
                        await _roleManager.AddClaimAsync(role, new Claim("permission", permission));
                    }
                }

                TempData["SuccessMessage"] = "Role updated successfully!";
                return RedirectToPage();
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            await LoadDataAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostDeleteAsync(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role != null)
            {
                var result = await _roleManager.DeleteAsync(role);
                if (result.Succeeded)
                {
                    TempData["SuccessMessage"] = "Role deleted successfully!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to delete role.";
                }
            }

            return RedirectToPage();
        }

        private async Task LoadDataAsync()
        {
            var roles = await _roleManager.Roles.ToListAsync();
            Roles = new List<RoleViewModel>();

            foreach (var role in roles)
            {
                var usersInRole = await _userManager.GetUsersInRoleAsync(role.Name!);
                var claims = await _roleManager.GetClaimsAsync(role);
                var permissions = claims.Where(c => c.Type == "permission").Select(c => c.Value).ToList();

                Roles.Add(new RoleViewModel
                {
                    Id = role.Id,
                    Name = role.Name!,
                    UserCount = usersInRole.Count,
                    Permissions = permissions
                });
            }

            // Define available permissions
            AvailablePermissions = new List<string>
            {
                "users.view", "users.create", "users.edit", "users.delete",
                "roles.view", "roles.create", "roles.edit", "roles.delete",
                "permissions.view", "permissions.create", "permissions.edit", "permissions.delete",
                "members.view", "members.create", "members.edit", "members.delete",
                "jobs.view", "jobs.create", "jobs.edit", "jobs.delete",
                "events.view", "events.create", "events.edit", "events.delete",
                "settings.view", "settings.edit",
                "reports.view", "reports.export"
            };
        }
    }
}
