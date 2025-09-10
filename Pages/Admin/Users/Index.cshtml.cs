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
    public class IndexModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public IndexModel(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public List<UserViewModel> Users { get; set; } = new();
        public List<IdentityRole> Roles { get; set; } = new();

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
        }

        public async Task<IActionResult> OnPostCreateAsync()
        {
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
                // Add roles
                if (CreateUser.SelectedRoles?.Any() == true)
                {
                    await _userManager.AddToRolesAsync(user, CreateUser.SelectedRoles);
                }

                TempData["SuccessMessage"] = "User created successfully!";
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
                // Update roles
                var currentRoles = await _userManager.GetRolesAsync(user);
                await _userManager.RemoveFromRolesAsync(user, currentRoles);
                
                if (EditUser.SelectedRoles?.Any() == true)
                {
                    await _userManager.AddToRolesAsync(user, EditUser.SelectedRoles);
                }

                TempData["SuccessMessage"] = "User updated successfully!";
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
            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                var result = await _userManager.DeleteAsync(user);
                if (result.Succeeded)
                {
                    TempData["SuccessMessage"] = "User deleted successfully!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to delete user.";
                }
            }

            return RedirectToPage();
        }

        private async Task LoadDataAsync()
        {
            var users = await _userManager.Users.ToListAsync();
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
