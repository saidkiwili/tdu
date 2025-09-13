using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using tae_app.Data;
using tae_app.Models;

namespace tae_app.Pages.Admin.Users
{
    [Authorize(Roles = "SuperAdmin,Admin")]
    public class IndexModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IAuthorizationService _authorizationService;
        private readonly ApplicationDbContext _context;

        public IndexModel(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IAuthorizationService authorizationService, ApplicationDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _authorizationService = authorizationService;
            _context = context;
        }

    public List<UserViewModel> Users { get; set; } = new();
        public List<IdentityRole> Roles { get; set; } = new();

    // Paging and filters
    [BindProperty(SupportsGet = true)]
    public string? Search { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? RoleFilter { get; set; }

    [BindProperty(SupportsGet = true)]
    public int PageIndex { get; set; } = 1;

    [BindProperty(SupportsGet = true)]
    public int PageSize { get; set; } = 10;

    public int TotalCount { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

        // Live statistics
        public int ActiveUsersCount { get; set; }
        public int AdministratorsCount { get; set; }
        public int BlockedUsersCount { get; set; }
    // UI permission flags
    public bool CanCreate { get; set; }
    public bool CanEdit { get; set; }
    public bool CanDelete { get; set; }

        [BindProperty]
        public CreateUserModel CreateUser { get; set; } = new();

        [BindProperty]
        public EditUserModel EditUser { get; set; } = new();

        public class UserViewModel
        {
            public string Id { get; set; } = "";
            public string Email { get; set; } = "";
            public string FirstName { get; set; } = "";
            public string LastName { get; set; } = "";
            public bool IsActive { get; set; }
            public DateTime CreatedAt { get; set; }
            public List<string> Roles { get; set; } = new();
        }

        public class CreateUserModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; } = "";

            [Required]
            public string FirstName { get; set; } = "";

            [Required]
            public string LastName { get; set; } = "";

            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; } = "";

            [Required]
            [DataType(DataType.Password)]
            [Compare("Password")]
            public string ConfirmPassword { get; set; } = "";

            public List<string> SelectedRoles { get; set; } = new();
        }

        public class EditUserModel
        {
            public string Id { get; set; } = "";
            public string Email { get; set; } = "";
            public string FirstName { get; set; } = "";
            public string LastName { get; set; } = "";
            public bool IsActive { get; set; }
            public List<string> SelectedRoles { get; set; } = new();
        }

        public async Task OnGetAsync()
        {
            await LoadDataAsync();

            // Evaluate current user's permissions for UI
            CanCreate = (await _authorizationService.AuthorizeAsync(User, "permission:users.create")).Succeeded;
            CanEdit = (await _authorizationService.AuthorizeAsync(User, "permission:users.edit")).Succeeded;
            CanDelete = (await _authorizationService.AuthorizeAsync(User, "permission:users.delete")).Succeeded;
        }

        public async Task<IActionResult> OnPostToggleBlockAsync(string id)
        {
            if (!(await _authorizationService.AuthorizeAsync(User, "permission:users.edit")).Succeeded)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    return new JsonResult(new { success = false, error = "forbidden" }) { StatusCode = 403 };
                return Forbid();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToPage();
            }

            user.IsActive = !user.IsActive;
            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = user.IsActive ? "User unblocked." : "User blocked.";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to update user status.";
            }

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return new JsonResult(new { success = true, isActive = user.IsActive });
            }

            return RedirectToPage(new { search = Search, roleFilter = RoleFilter, pageIndex = PageIndex });
        }

        public async Task<IActionResult> OnPostCreateAsync()
        {
            if (!(await _authorizationService.AuthorizeAsync(User, "permission:users.create")).Succeeded)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    return new JsonResult(new { success = false, error = "forbidden" }) { StatusCode = 403 };
                return Forbid();
            }

            if (!ModelState.IsValid)
            {
                await LoadDataAsync();
                return Page();
            }

            var user = new ApplicationUser
            {
                UserName = CreateUser.Email,
                Email = CreateUser.Email,
                FirstName = CreateUser.FirstName,
                LastName = CreateUser.LastName,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, CreateUser.Password);
            if (result.Succeeded)
            {
                if (CreateUser.SelectedRoles?.Any() == true)
                {
                    await _userManager.AddToRolesAsync(user, CreateUser.SelectedRoles);
                }

                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return new JsonResult(new { success = true, id = user.Id, email = user.Email });
                }

                TempData["SuccessMessage"] = "User created successfully!";
                return RedirectToPage();
            }

            var errors = result.Errors.Select(e => e.Description).ToList();
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return new JsonResult(new { success = false, errors });
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
            if (!(await _authorizationService.AuthorizeAsync(User, "permission:users.edit")).Succeeded)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    return new JsonResult(new { success = false, error = "forbidden" }) { StatusCode = 403 };
                return Forbid();
            }

            if (!ModelState.IsValid)
            {
                await LoadDataAsync();
                return Page();
            }

            var user = await _userManager.FindByIdAsync(EditUser.Id);
            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found!";
                return RedirectToPage();
            }

            user.FirstName = EditUser.FirstName;
            user.LastName = EditUser.LastName;
            user.IsActive = EditUser.IsActive;

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                var currentRoles = await _userManager.GetRolesAsync(user);
                await _userManager.RemoveFromRolesAsync(user, currentRoles);

                if (EditUser.SelectedRoles?.Any() == true)
                {
                    await _userManager.AddToRolesAsync(user, EditUser.SelectedRoles);
                }

                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return new JsonResult(new { success = true });
                }

                TempData["SuccessMessage"] = "User updated successfully!";
                return RedirectToPage();
            }

            var errors = result.Errors.Select(e => e.Description).ToList();
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return new JsonResult(new { success = false, errors });
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
            if (!(await _authorizationService.AuthorizeAsync(User, "permission:users.delete")).Succeeded)
            {
                return Forbid();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                // Find and delete associated member record
                var member = await _context.Members.FirstOrDefaultAsync(m => m.ApplicationUserId == id);
                if (member != null)
                {
                    _context.Members.Remove(member);
                    await _context.SaveChangesAsync();
                }

                // Delete the user
                var result = await _userManager.DeleteAsync(user);
                if (result.Succeeded)
                {
                    TempData["SuccessMessage"] = "User and associated member record deleted successfully!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to delete user.";
                }
            }

            return RedirectToPage();
        }

        // Return user's roles as JSON for AJAX prefill
        public async Task<IActionResult> OnGetUserRolesAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return BadRequest();
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();
            var roles = await _userManager.GetRolesAsync(user);
            return new JsonResult(roles);
        }

        private async Task LoadDataAsync()
        {
            var query = _userManager.Users.AsQueryable();

            // Exclude SuperAdmin users from the list
            var superAdminUsers = await _userManager.GetUsersInRoleAsync("SuperAdmin");
            var superAdminIds = superAdminUsers.Select(u => u.Id).ToList();
            query = query.Where(u => !superAdminIds.Contains(u.Id));

            // Search
            if (!string.IsNullOrWhiteSpace(Search))
            {
                var s = Search.Trim().ToLower();
                query = query.Where(u => (u.Email ?? "").ToLower().Contains(s)
                                          || (u.FirstName ?? "").ToLower().Contains(s)
                                          || (u.LastName ?? "").ToLower().Contains(s));
            }

            // Role filter - fetch ids that match the role
            if (!string.IsNullOrWhiteSpace(RoleFilter))
            {
                var usersInRole = await _userManager.GetUsersInRoleAsync(RoleFilter);
                var ids = usersInRole.Select(u => u.Id).ToList();
                query = query.Where(u => ids.Contains(u.Id));
            }

            TotalCount = await query.CountAsync();

            // Live stats - calculate from filtered query to avoid negative numbers
            ActiveUsersCount = await query.CountAsync(u => u.IsActive);

            // Count admins (if role exists) - also from filtered query
            AdministratorsCount = 0;
            if (await _roleManager.RoleExistsAsync("Admin"))
            {
                var adminUsersInFilteredQuery = await _userManager.GetUsersInRoleAsync("Admin");
                var adminIds = adminUsersInFilteredQuery.Select(u => u.Id).ToList();
                AdministratorsCount = await query.CountAsync(u => adminIds.Contains(u.Id));
            }

            // Calculate blocked users from the filtered query
            BlockedUsersCount = TotalCount - ActiveUsersCount;

            var users = await query
                .OrderByDescending(u => u.CreatedAt)
                .Skip((PageIndex - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            Users = new List<UserViewModel>();
            foreach (var user in users)
            {
                var userRoles = await _userManager.GetRolesAsync(user);
                Users.Add(new UserViewModel
                {
                    Id = user.Id,
                    Email = user.Email ?? "",
                    FirstName = user.FirstName ?? "",
                    LastName = user.LastName ?? "",
                    IsActive = user.IsActive,
                    CreatedAt = user.CreatedAt,
                    Roles = userRoles.ToList()
                });
            }

            Roles = await _roleManager.Roles.ToListAsync();
        }
    }
}
