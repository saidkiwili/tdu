using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using tae_app.Models;
using tae_app.Data;

namespace tae_app.Pages.Admin
{
    [Authorize(Roles = "SuperAdmin,Admin")]
    public class DashboardModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;

        public DashboardModel(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }

        public DashboardStats Stats { get; set; } = new();

        public class DashboardStats
        {
            public int TotalUsers { get; set; }
            public int TotalRoles { get; set; }
            public int TotalMembers { get; set; }
            public int ActiveUsers { get; set; }
            public List<RecentActivity> RecentActivities { get; set; } = new();
        }

        public class RecentActivity
        {
            public string Action { get; set; } = "";
            public string User { get; set; } = "";
            public DateTime Timestamp { get; set; }
            public string Type { get; set; } = "";
        }

        public async Task OnGetAsync()
        {
            // Get dashboard statistics
            Stats.TotalUsers = await _userManager.Users.CountAsync();
            Stats.TotalRoles = await _roleManager.Roles.CountAsync();
            Stats.ActiveUsers = await _userManager.Users.Where(u => u.IsActive).CountAsync();
            
            // Get recent activities (mock data for now)
            Stats.RecentActivities = new List<RecentActivity>
            {
                new() { Action = "User registered", User = "John Doe", Timestamp = DateTime.Now.AddMinutes(-10), Type = "info" },
                new() { Action = "Role created", User = "Admin", Timestamp = DateTime.Now.AddHours(-2), Type = "success" },
                new() { Action = "Permission updated", User = "Admin", Timestamp = DateTime.Now.AddHours(-4), Type = "warning" }
            };
        }
    }
}
