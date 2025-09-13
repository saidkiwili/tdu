using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace tae_app.Pages.Admin.Roles
{
    [Authorize(Roles = "SuperAdmin,Admin")]
    public class EditModel : PageModel
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IAuthorizationService _authorizationService;

        public EditModel(RoleManager<IdentityRole> roleManager, IAuthorizationService authorizationService)
        {
            _roleManager = roleManager;
            _authorizationService = authorizationService;
        }

        [BindProperty(SupportsGet = true)]
        public string Id { get; set; } = "";

        [BindProperty]
        public EditRoleModel EditRole { get; set; } = new();

        public List<string> AvailablePermissions { get; set; } = new();

        public class EditRoleModel
        {
            public string Id { get; set; } = "";
            [Required]
            public string Name { get; set; } = "";
            public List<string> SelectedPermissions { get; set; } = new();
        }

        public async Task<IActionResult> OnGetAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                TempData["ErrorMessage"] = "Invalid role ID.";
                return RedirectToPage("/Admin/Roles/Index");
            }

            Id = id;
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
            {
                TempData["ErrorMessage"] = "Role not found.";
                return RedirectToPage("/Admin/Roles/Index");
            }

            EditRole.Id = role.Id;
            EditRole.Name = role.Name ?? "";
            var claims = await _roleManager.GetClaimsAsync(role);
            EditRole.SelectedPermissions = claims.Where(c => c.Type == "permission").Select(c => c.Value).ToList();

            LoadPermissions();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!(await _authorizationService.AuthorizeAsync(User, "permission:roles.edit")).Succeeded)
            {
                TempData["ErrorMessage"] = "You do not have permission to edit roles.";
                return RedirectToPage("/Admin/Roles/Index");
            }

            LoadPermissions();

            if (!ModelState.IsValid)
            {
                return Page();
            }

            var role = await _roleManager.FindByIdAsync(EditRole.Id);
            if (role == null)
            {
                TempData["ErrorMessage"] = "Role not found.";
                return RedirectToPage("/Admin/Roles/Index");
            }

            role.Name = EditRole.Name;
            var result = await _roleManager.UpdateAsync(role);
            if (!result.Succeeded)
            {
                foreach (var err in result.Errors) ModelState.AddModelError(string.Empty, err.Description);
                return Page();
            }

            // Update permissions
            var existingClaims = await _roleManager.GetClaimsAsync(role);
            var permissionClaims = existingClaims.Where(c => c.Type == "permission").ToList();
            foreach (var claim in permissionClaims)
            {
                await _roleManager.RemoveClaimAsync(role, claim);
            }

            if (EditRole.SelectedPermissions?.Any() == true)
            {
                foreach (var p in EditRole.SelectedPermissions)
                {
                    await _roleManager.AddClaimAsync(role, new Claim("permission", p));
                }
            }

            TempData["SuccessMessage"] = "Role updated successfully!";
            return RedirectToPage("/Admin/Roles/Index");
        }

        private void LoadPermissions()
        {
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
