using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using tae_app.Models;

namespace tae_app.Services
{
    public class RoleClaimsAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public RoleClaimsAuthorizationHandler(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
        {
            var principal = context.User;
            if (principal == null || !(principal.Identity?.IsAuthenticated ?? false))
            {
                return;
            }

            var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId)) return;

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return;

            var roles = await _userManager.GetRolesAsync(user);
            foreach (var roleName in roles)
            {
                // Treat SuperAdmin as implicitly granted all permissions
                if (string.Equals(roleName, "SuperAdmin", StringComparison.OrdinalIgnoreCase))
                {
                    context.Succeed(requirement);
                    return;
                }
                var role = await _roleManager.FindByNameAsync(roleName);
                if (role == null) continue;

                var claims = await _roleManager.GetClaimsAsync(role);
                if (claims.Any(c => c.Type == "permission" && c.Value == requirement.Permission))
                {
                    context.Succeed(requirement);
                    return;
                }
            }
        }
    }
}
