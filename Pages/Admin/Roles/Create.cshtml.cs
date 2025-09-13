using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace tae_app.Pages.Admin.Roles
{
    [Authorize(Roles = "SuperAdmin,Admin")]
    public class CreateModel : PageModel
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IAuthorizationService _authorizationService;

        public CreateModel(RoleManager<IdentityRole> roleManager, IAuthorizationService authorizationService)
        {
            _roleManager = roleManager;
            _authorizationService = authorizationService;
        }

        [BindProperty]
        public CreateRoleModel CreateRole { get; set; } = new();

        public List<string> AvailablePermissions { get; set; } = new();

        public class CreateRoleModel
        {
            [Required]
            public string Name { get; set; } = "";
            public List<string> SelectedPermissions { get; set; } = new();
        }

        public void OnGet()
        {
            LoadPermissions();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!(await _authorizationService.AuthorizeAsync(User, "permission:roles.create")).Succeeded)
            {
                TempData["ErrorMessage"] = "You do not have permission to create roles.";
                return RedirectToPage("/Admin/Roles/Index");
            }

            LoadPermissions();

            if (!ModelState.IsValid)
            {
                return Page();
            }

            var role = new IdentityRole(CreateRole.Name);
            var result = await _roleManager.CreateAsync(role);
            if (!result.Succeeded)
            {
                foreach (var err in result.Errors) ModelState.AddModelError(string.Empty, err.Description);
                return Page();
            }

            if (CreateRole.SelectedPermissions?.Any() == true)
            {
                foreach (var p in CreateRole.SelectedPermissions)
                {
                    await _roleManager.AddClaimAsync(role, new Claim("permission", p));
                }
            }

            TempData["SuccessMessage"] = "Role created successfully!";
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
