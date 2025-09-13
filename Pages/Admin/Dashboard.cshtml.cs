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
        private readonly IAuthorizationService _authorizationService;

        public DashboardModel(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext context,
            IAuthorizationService authorizationService)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
            _authorizationService = authorizationService;
        }

        public DashboardStats Stats { get; set; } = new();

        public class DashboardStats
        {
            public int TotalUsers { get; set; }
            public int TotalRoles { get; set; }
            public int TotalMembers { get; set; }
            public int ActiveUsers { get; set; }
            public int TotalAppointments { get; set; }
            public int PendingAppointments { get; set; }
            public int CompletedAppointments { get; set; }
            public int RecentRegistrations { get; set; }
            public List<RecentActivity> RecentActivities { get; set; } = new();
            public List<string> MonthLabels { get; set; } = new();
            public List<int> MonthCounts { get; set; } = new();
        }

        public class RecentActivity
        {
            public string Action { get; set; } = "";
            public string User { get; set; } = "";
            public DateTime Timestamp { get; set; }
            public string Type { get; set; } = "";
            public string Status { get; set; } = "Active";
        }

        public async Task OnGetAsync()
        {
            try
            {
                // Get dashboard statistics
                Stats.TotalUsers = await _userManager.Users.CountAsync();
                Stats.TotalRoles = await _roleManager.Roles.CountAsync();
                Stats.ActiveUsers = await _userManager.Users.Where(u => u.IsActive).CountAsync();
                Stats.TotalMembers = await _context.Members.CountAsync();

                // Appointments statistics
                Stats.TotalAppointments = await _context.Appointments.CountAsync();
                Stats.PendingAppointments = await _context.Appointments
                    .Where(a => a.Status == AppointmentStatus.Scheduled || a.Status == AppointmentStatus.Confirmed)
                    .CountAsync();
                Stats.CompletedAppointments = await _context.Appointments
                    .Where(a => a.Status == AppointmentStatus.Completed)
                    .CountAsync();

                // Recent registrations (last 30 days)
                var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
                Stats.RecentRegistrations = await _context.Members
                    .Where(m => m.CreatedAt >= thirtyDaysAgo)
                    .CountAsync();

                // Recent activities: recent member registrations (last 10 items)
                var recentRegs = await _context.Members
                    .OrderByDescending(m => m.CreatedAt)
                    .Take(10)
                    .Select(m => new RecentActivity {
                        Action = "Member registered",
                        User = m.FullName,
                        Timestamp = m.CreatedAt,
                        Type = m.OptInNidaService ? "NIDA Service" : "Registration",
                        Status = m.OptInNidaService ?
                            (m.NidaServiceStatus == NidaServiceStatus.Completed ? "Completed" :
                             m.NidaServiceStatus == NidaServiceStatus.AppointmentScheduled ? "Appointment" :
                             m.NidaServiceStatus == NidaServiceStatus.Paid ? "Paid" : "Pending") : "Active"
                    })
                    .ToListAsync();
                Stats.RecentActivities = recentRegs ?? new List<RecentActivity>();

                // Member registrations per month (last 12 months)
                var now = DateTime.UtcNow;

                var months = Enumerable.Range(0, 12)
                    .Select(i => new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(-i))
                    .Reverse()
                    .ToList();

                var monthLabels = months.Select(m => m.ToString("MMM yyyy")).ToList();
                var monthCounts = new List<int>();

                foreach (var m in months)
                {
                    var start = new DateTime(m.Year, m.Month, 1, 0, 0, 0, DateTimeKind.Utc);
                    var end = start.AddMonths(1);
                    var cnt = await _context.Members.Where(x => x.CreatedAt >= start && x.CreatedAt < end).CountAsync();
                    // Ensure count is valid (not negative, not infinite)
                    cnt = Math.Max(0, Math.Min(cnt, int.MaxValue));
                    monthCounts.Add(cnt);
                }

                Stats.MonthLabels = monthLabels ?? new List<string>();
                Stats.MonthCounts = monthCounts ?? new List<int>();

                // UI flags for which management cards to enable
                CanViewUsers = (await _authorizationService.AuthorizeAsync(User, "permission:users.view")).Succeeded;
                CanViewMembers = (await _authorizationService.AuthorizeAsync(User, "permission:members.view")).Succeeded;
                CanViewEvents = (await _authorizationService.AuthorizeAsync(User, "permission:events.view")).Succeeded;
                CanViewReports = (await _authorizationService.AuthorizeAsync(User, "permission:reports.view")).Succeeded;
            }
            catch (Exception ex)
            {
                // Log the error and provide fallback data
                Console.WriteLine($"Dashboard error: {ex.Message}");
                Stats.MonthLabels = new List<string>();
                Stats.MonthCounts = new List<int>();
                Stats.RecentActivities = new List<RecentActivity>();
            }
        }

        // UI permission flags
        public bool CanViewUsers { get; set; }
        public bool CanViewMembers { get; set; }
        public bool CanViewEvents { get; set; }
        public bool CanViewReports { get; set; }

        // Helper methods for the view
        public string GetInitials(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName))
                return "U";

            var parts = fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 2)
            {
                return $"{parts[0][0]}{parts[1][0]}".ToUpper();
            }
            else if (parts.Length == 1)
            {
                return parts[0].Length >= 2 ? $"{parts[0][0]}{parts[0][1]}".ToUpper() : $"{parts[0][0]}".ToUpper();
            }
            return "U";
        }

        public string GetTimeAgo(DateTime timestamp)
        {
            var now = DateTime.UtcNow;
            var timeSpan = now - timestamp;

            if (timeSpan.TotalMinutes < 1)
                return "just now";
            if (timeSpan.TotalMinutes < 60)
                return $"{(int)timeSpan.TotalMinutes} minutes ago";
            if (timeSpan.TotalHours < 24)
                return $"{(int)timeSpan.TotalHours} hours ago";
            if (timeSpan.TotalDays < 30)
                return $"{(int)timeSpan.TotalDays} days ago";
            if (timeSpan.TotalDays < 365)
                return $"{(int)(timeSpan.TotalDays / 30)} months ago";
            return $"{(int)(timeSpan.TotalDays / 365)} years ago";
        }

        public string GetAvatarClass(int index)
        {
            var gradients = new[]
            {
                "from-tanzanian-green to-tanzanian-blue",
                "from-tanzanian-blue to-uae-red",
                "from-uae-red to-tanzanian-green",
                "from-emerald-500 to-emerald-600",
                "from-purple-500 to-purple-600",
                "from-blue-500 to-blue-600"
            };
            return gradients[index % gradients.Length];
        }

        public string GetStatusClass(string status)
        {
            return status.ToLower() switch
            {
                "completed" => "text-green-700 bg-green-100",
                "paid" => "text-blue-700 bg-blue-100",
                "appointment" => "text-purple-700 bg-purple-100",
                "pending" => "text-amber-700 bg-amber-100",
                "active" => "text-emerald-700 bg-emerald-100",
                _ => "text-gray-700 bg-gray-100"
            };
        }
    }
}