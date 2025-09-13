using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;

namespace tae_app.Pages.Admin.Permissions
{
    [Authorize(Roles = "SuperAdmin,Admin")]
    public class IndexModel : PageModel
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IAuthorizationService _authorizationService;

        public IndexModel(RoleManager<IdentityRole> roleManager, IAuthorizationService authorizationService)
        {
            _roleManager = roleManager;
            _authorizationService = authorizationService;
        }

        public Dictionary<string, List<PermissionItem>> PermissionCategories { get; set; } = new();
        public List<RolePermissionViewModel> Roles { get; set; } = new();
        public List<string> AllPermissions { get; set; } = new();

        public class PermissionItem
        {
            public string Name { get; set; } = "";
            public string Description { get; set; } = "";
            public List<string> AssignedRoles { get; set; } = new();
        }

        public class RolePermissionViewModel
        {
            public string Id { get; set; } = "";
            public string Name { get; set; } = "";
            public List<string> Permissions { get; set; } = new();
        }

        public async Task OnGetAsync()
        {
            await LoadDataAsync();
            CanEdit = (await _authorizationService.AuthorizeAsync(User, "permission:permissions.edit")).Succeeded;
        }

        public bool CanEdit { get; set; }

        public async Task<IActionResult> OnPostUpdatePermissionAsync([FromBody] UpdatePermissionRequest request)
        {
            if (!(await _authorizationService.AuthorizeAsync(User, "permission:permissions.edit")).Succeeded)
            {
                return Forbid();
            }

            try
            {
                var role = await _roleManager.FindByIdAsync(request.RoleId);
                if (role == null)
                {
                    return BadRequest("Role not found");
                }

                var claims = await _roleManager.GetClaimsAsync(role);
                var existingClaim = claims.FirstOrDefault(c => c.Type == "permission" && c.Value == request.Permission);

                if (request.IsAssigned && existingClaim == null)
                {
                    // Add permission
                    await _roleManager.AddClaimAsync(role, new Claim("permission", request.Permission));
                }
                else if (!request.IsAssigned && existingClaim != null)
                {
                    // Remove permission
                    await _roleManager.RemoveClaimAsync(role, existingClaim);
                }

                return new JsonResult(new { success = true });
            }
            catch (Exception ex)
            {
                return BadRequest($"Error: {ex.Message}");
            }
        }

        private async Task LoadDataAsync()
        {
            // Define all available permissions
            AllPermissions = new List<string>
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

            // Group permissions by category
            PermissionCategories = AllPermissions
                .GroupBy(p => p.Split('.')[0])
                .ToDictionary(
                    g => g.Key.ToUpperInvariant(),
                    g => g.Select(p => new PermissionItem
                    {
                        Name = p,
                        Description = GetPermissionDescription(p),
                        AssignedRoles = new List<string>()
                    }).ToList()
                );

            // Load roles and their permissions
            var roles = await _roleManager.Roles.ToListAsync();
            Roles = new List<RolePermissionViewModel>();

            foreach (var role in roles)
            {
                var claims = await _roleManager.GetClaimsAsync(role);
                var permissions = claims.Where(c => c.Type == "permission").Select(c => c.Value).ToList();

                Roles.Add(new RolePermissionViewModel
                {
                    Id = role.Id,
                    Name = role.Name!,
                    Permissions = permissions
                });

                // Update permission categories with role assignments
                foreach (var permission in permissions)
                {
                    var category = permission.Split('.')[0].ToUpperInvariant();
                    if (PermissionCategories.ContainsKey(category))
                    {
                        var permissionItem = PermissionCategories[category].FirstOrDefault(p => p.Name == permission);
                        if (permissionItem != null)
                        {
                            permissionItem.AssignedRoles.Add(role.Name!);
                        }
                    }
                }
            }
        }

        private string GetPermissionDescription(string permission)
        {
            var parts = permission.Split('.');
            if (parts.Length != 2) return "System permission";

            var resource = parts[0].ToUpperInvariant();
            var action = parts[1].ToLowerInvariant();

            return action switch
            {
                "view" => $"View {resource}",
                "create" => $"Create {resource}",
                "edit" => $"Edit {resource}",
                "delete" => $"Delete {resource}",
                "export" => $"Export {resource}",
                _ => $"{action.ToUpperInvariant()} {resource}"
            };
        }

        public class UpdatePermissionRequest
        {
            public string RoleId { get; set; } = "";
            public string Permission { get; set; } = "";
            public bool IsAssigned { get; set; }
        }
    }
}
