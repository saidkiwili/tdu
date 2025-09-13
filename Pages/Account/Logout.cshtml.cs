using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using tae_app.Models;

namespace tae_app.Pages.Account;

public class LogoutModel : PageModel
{
    private readonly SignInManager<ApplicationUser> _signInManager;

    public LogoutModel(SignInManager<ApplicationUser> signInManager)
    {
        _signInManager = signInManager;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await _signInManager.SignOutAsync();
        return Redirect("/");
    }

    public async Task<IActionResult> OnGetAsync()
    {
        // Perform logout immediately on GET request
        if (User?.Identity?.IsAuthenticated == true)
        {
            await _signInManager.SignOutAsync();
        }

        // Redirect to home page immediately
        return Redirect("/");
    }
}

