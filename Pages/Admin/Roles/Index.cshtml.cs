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
        private readonly IAuthorizationService _authorizationService;

        public IndexModel(RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager, IAuthorizationService authorizationService)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _authorizationService = authorizationService;
        }

    public List<RoleViewModel> Roles { get; set; } = new();
        public List<string> AvailablePermissions { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public string? Search { get; set; }

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
            // Ensure 'Member' role exists
            if (!await _roleManager.RoleExistsAsync("Member"))
            {
                await _roleManager.CreateAsync(new IdentityRole("Member"));
            }

            CanCreate = (await _authorizationService.AuthorizeAsync(User, "permission:roles.create")).Succeeded;
            CanEdit = (await _authorizationService.AuthorizeAsync(User, "permission:roles.edit")).Succeeded;
            CanDelete = (await _authorizationService.AuthorizeAsync(User, "permission:roles.delete")).Succeeded;
        }

        public async Task<IActionResult> OnPostCreateAsync()
        {
            var isAjax = Request.Headers["X-Requested-With"] == "XMLHttpRequest";

            // Debug: Print permissions for Admin role
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                var roles = await _userManager.GetRolesAsync(user);
                foreach (var roleName in roles)
                {
                    if (string.Equals(roleName, "Admin", StringComparison.OrdinalIgnoreCase))
                    {
                        var adminRole = await _roleManager.FindByNameAsync(roleName);
                        if (adminRole != null)
                        {
                            var claims = await _roleManager.GetClaimsAsync(adminRole);
                            var permissions = claims.Where(c => c.Type == "permission").Select(c => c.Value).ToList();
                            Console.WriteLine($"Admin role permissions: {string.Join(", ", permissions)}");
                        }
                    }
                }
            }

            if (!(await _authorizationService.AuthorizeAsync(User, "permission:roles.create")).Succeeded)
            {
                if (isAjax)
                {
                    return new JsonResult(new { success = false, message = "Forbidden" }) { StatusCode = StatusCodes.Status403Forbidden };
                }
                // For standard form posts, show a friendly message and redirect back
                TempData["ErrorMessage"] = "You do not have permission to create roles.";
                await LoadDataAsync();
                return RedirectToPage();
            }

            if (!ModelState.IsValid)
            {
                // Attempt to recover values from Request.Form when AJAX FormData binding doesn't populate nested models
                if (Request.HasFormContentType)
                {
                    // Create handler recovery
                    var nameKey = "CreateRole.Name";
                    if (string.IsNullOrWhiteSpace(CreateRole?.Name) && Request.Form.ContainsKey(nameKey))
                    {
                        CreateRole.Name = Request.Form[nameKey];
                    }
                    var permsKey = "CreateRole.SelectedPermissions";
                    if ((CreateRole?.SelectedPermissions == null || !CreateRole.SelectedPermissions.Any()) && Request.Form.ContainsKey(permsKey))
                    {
                        CreateRole.SelectedPermissions = Request.Form[permsKey].ToList();
                    }

                    // Re-run validation for CreateRole
                    ModelState.Clear();
                    TryValidateModel(CreateRole, nameof(CreateRole));
                }

                // If still invalid, return errors
                if (isAjax)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                    return new JsonResult(new { success = false, errors });
                }

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

                if (isAjax)
                {
                    return new JsonResult(new { success = true, message = "Role created successfully!" });
                }

                TempData["SuccessMessage"] = "Role created successfully!";
                return RedirectToPage();
            }

            var errorsList = result.Errors.Select(e => e.Description).ToList();
            if (isAjax)
            {
                // Include ModelState and form values to aid debugging when AJAX callers send FormData
                var modelErrors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                var formData = Request.HasFormContentType ? Request.Form.ToDictionary(k => k.Key, v => v.Value.ToString()) : null;
                return new JsonResult(new { success = false, errors = errorsList, modelErrors, form = formData });
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            if (isAjax)
            {
                var modelErrors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                var formData = Request.HasFormContentType ? Request.Form.ToDictionary(k => k.Key, v => v.Value.ToString()) : null;
                return new JsonResult(new { success = false, errors = result.Errors.Select(e => e.Description).ToList(), modelErrors, form = formData });
            }

            await LoadDataAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostEditAsync()
        {
            var isAjax = Request.Headers["X-Requested-With"] == "XMLHttpRequest";
            if (!(await _authorizationService.AuthorizeAsync(User, "permission:roles.edit")).Succeeded)
            {
                if (isAjax)
                {
                    return new JsonResult(new { success = false, message = "Forbidden" }) { StatusCode = StatusCodes.Status403Forbidden };
                }
                TempData["ErrorMessage"] = "You do not have permission to edit roles.";
                await LoadDataAsync();
                return RedirectToPage();
            }

            if (!ModelState.IsValid)
            {
                // Attempt to recover values from Request.Form when AJAX FormData binding doesn't populate nested models
                if (Request.HasFormContentType)
                {
                    var idKey = "EditRole.Id";
                    var nameKey = "EditRole.Name";
                    var permsKey = "EditRole.SelectedPermissions";
                    if (string.IsNullOrWhiteSpace(EditRole?.Id) && Request.Form.ContainsKey(idKey))
                    {
                        EditRole.Id = Request.Form[idKey];
                    }
                    if (string.IsNullOrWhiteSpace(EditRole?.Name) && Request.Form.ContainsKey(nameKey))
                    {
                        EditRole.Name = Request.Form[nameKey];
                    }
                    if ((EditRole?.SelectedPermissions == null || !EditRole.SelectedPermissions.Any()) && Request.Form.ContainsKey(permsKey))
                    {
                        EditRole.SelectedPermissions = Request.Form[permsKey].ToList();
                    }

                    // Re-run validation for EditRole
                    ModelState.Clear();
                    TryValidateModel(EditRole, nameof(EditRole));
                }

                if (isAjax)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                    return new JsonResult(new { success = false, errors });
                }

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

                if (isAjax)
                {
                    return new JsonResult(new { success = true, message = "Role updated successfully!" });
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
            var isAjax = Request.Headers["X-Requested-With"] == "XMLHttpRequest";
            if (!(await _authorizationService.AuthorizeAsync(User, "permission:roles.delete")).Succeeded)
            {
                if (isAjax)
                {
                    return new JsonResult(new { success = false, message = "Forbidden" }) { StatusCode = StatusCodes.Status403Forbidden };
                }
                TempData["ErrorMessage"] = "You do not have permission to delete roles.";
                await LoadDataAsync();
                return RedirectToPage();
            }

            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
            {
                TempData["ErrorMessage"] = "Role not found.";
                return RedirectToPage();
            }

            // Prevent deleting core roles
            var protectedRoles = new[] { "Member", "Admin", "SuperAdmin" };
            if (protectedRoles.Contains(role.Name))
            {
                TempData["ErrorMessage"] = "This role cannot be deleted.";
                return RedirectToPage();
            }

            // Prevent deleting roles that have users
            var usersInRole = await _userManager.GetUsersInRoleAsync(role.Name!);
            if (usersInRole.Count > 0)
            {
                TempData["ErrorMessage"] = "Cannot delete a role that has assigned users.";
                return RedirectToPage();
            }

            var result = await _roleManager.DeleteAsync(role);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Role deleted successfully!";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to delete role.";
            }

            return RedirectToPage();
        }

        // Return permissions for a role as JSON for client-side pre-checking
        public async Task<IActionResult> OnGetGetRolePermissionsAsync(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null) return NotFound();

            // If role is SuperAdmin, return all available permissions so UI pre-checks them
            if (string.Equals(role.Name, "SuperAdmin", StringComparison.OrdinalIgnoreCase))
            {
                return new JsonResult(new { permissions = AvailablePermissions });
            }

            var claims = await _roleManager.GetClaimsAsync(role);
            var permissions = claims.Where(c => c.Type == "permission").Select(c => c.Value).ToList();
            return new JsonResult(new { permissions });
        }

        // UI permission flags
        public bool CanCreate { get; set; }
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }

        private async Task LoadDataAsync()
        {
            var roles = await _roleManager.Roles.ToListAsync();
            Roles = new List<RoleViewModel>();
            // filter by Search if provided
            if (!string.IsNullOrWhiteSpace(Search))
            {
                roles = roles.Where(r => r.Name != null && r.Name.Contains(Search, StringComparison.OrdinalIgnoreCase)).ToList();
            }
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
