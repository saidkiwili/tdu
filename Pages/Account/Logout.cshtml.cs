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
        return RedirectToPage("/Index");
    }

    public IActionResult OnGet()
    {
        return RedirectToPage("/Index");
    }
}
