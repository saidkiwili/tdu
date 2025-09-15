using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using tae_app.Services;
using tae_app.Data;
using tae_app.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace tae_app.Pages.Account;

public class VerifyOtpModel : PageModel
{
    private readonly IOtpService _otpService;
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<VerifyOtpModel> _logger;

    public VerifyOtpModel(
        IOtpService otpService,
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IWebHostEnvironment environment,
        ILogger<VerifyOtpModel> logger)
    {
        _otpService = otpService;
        _context = context;
        _userManager = userManager;
        _signInManager = signInManager;
        _environment = environment;
        _logger = logger;
    }

    [BindProperty]
    public string Email { get; set; } = string.Empty;

    [BindProperty]
    public string Otp { get; set; } = string.Empty;

    [BindProperty]
    public string[] OtpDigits { get; set; } = new string[4];

    public bool ShowEmailReminder { get; set; } = true;

    public async Task<IActionResult> OnGetAsync(string email)
    {
        if (string.IsNullOrEmpty(email))
        {
            TempData["Error"] = "Invalid request. Email is required.";
            return RedirectToPage("/Account/Register");
        }

        Email = email;
        ShowEmailReminder = true; // Show reminder on initial page load

        // Check if there's a valid OTP for this email
        var hasValidOtp = await _context.OtpVerifications
            .AnyAsync(o => o.Email == email && !o.IsUsed && o.ExpiresAt > DateTime.UtcNow);

        if (!hasValidOtp)
        {
            TempData["Error"] = "No valid verification code found. Please request a new one.";
            return RedirectToPage("/Account/Register");
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        _logger.LogInformation("=== OTP VERIFICATION POST STARTED ===");

        _logger.LogInformation("OTP verification attempt for email: {Email}", Email);

        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Model state is invalid for email: {Email}", Email);
            ShowEmailReminder = false; // Don't show reminder on validation errors
            return Page();
        }

        // Combine OTP digits
        Otp = string.Join("", OtpDigits);
        _logger.LogInformation("OTP Digits received: [{Digits}] for email: {Email}", string.Join(",", OtpDigits), Email);
        _logger.LogInformation("Combined OTP: {Otp} for email: {Email}", Otp, Email);

        if (string.IsNullOrEmpty(Otp) || Otp.Length != 4)
        {
            _logger.LogWarning("Invalid OTP length: {Length} for email: {Email}", Otp?.Length ?? 0, Email);
            ModelState.AddModelError("", "Please enter a valid 4-digit verification code.");
            ShowEmailReminder = false; // Don't show reminder on validation errors
            return Page();
        }

        try
        {
            // Verify OTP
            _logger.LogInformation("Verifying OTP: {Otp} for email: {Email}", Otp, Email);
            var isValid = await _otpService.VerifyOtpAsync(Email, Otp);

            if (!isValid)
            {
                _logger.LogWarning("OTP verification failed for email: {Email}", Email);
                ModelState.AddModelError("", "Invalid or expired verification code. Please try again.");
                ShowEmailReminder = false; // Don't show reminder on invalid OTP
                return Page();
            }

            _logger.LogInformation("OTP verified successfully for email: {Email}", Email);

            // Get the OTP record to retrieve registration data
            var otpRecord = await _context.OtpVerifications
                .FirstOrDefaultAsync(o => o.Email == Email && o.Code == Otp && o.IsUsed);

            if (otpRecord == null)
            {
                _logger.LogError("OTP record not found after verification for email: {Email}, code: {Code}", Email, Otp);
                TempData["Error"] = "Verification completed but registration data not found. Please contact support.";
                return RedirectToPage("/Account/Register");
            }

            if (string.IsNullOrEmpty(otpRecord.RegistrationData))
            {
                _logger.LogError("OTP record found but registration data is empty for email: {Email}", Email);
                TempData["Error"] = "Registration data not found. Please start the registration process again.";
                return RedirectToPage("/Account/Register");
            }

            // Deserialize registration data
            var registrationData = JsonSerializer.Deserialize<RegistrationData>(otpRecord.RegistrationData);

            if (registrationData == null)
            {
                TempData["Error"] = "Invalid registration data. Please start the registration process again.";
                return RedirectToPage("/Account/Register");
            }

            // Complete the registration process
            _logger.LogInformation("Completing registration for email: {Email}", Email);
            var result = await CompleteRegistrationAsync(registrationData);

            if (result.Succeeded)
            {
                _logger.LogInformation("Registration completed successfully for email: {Email}", Email);
                TempData["SuccessMessage"] = $"🎉 Registration successful! Your email has been verified and your account has been created. Welcome to TDUAE! Your member ID is: {registrationData.MemberId}. You can now access all member features.";
                return RedirectToPage("/Index");
            }
            else
            {
                _logger.LogError("Registration failed for email: {Email}. Errors: {Errors}", Email, string.Join(", ", result.Errors.Select(e => e.Description)));
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
                return Page();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying OTP for email: {Email}", Email);
            ModelState.AddModelError("", "An error occurred while verifying your code. Please try again.");
            ShowEmailReminder = false; // Don't show reminder on exceptions
            return Page();
        }
    }

    public async Task<IActionResult> OnPostResendAsync()
    {
        try
        {
            var success = await _otpService.ResendOtpAsync(Email);

            if (success)
            {
                return new JsonResult(new { success = true, message = "Verification code sent successfully." });
            }
            else
            {
                return new JsonResult(new { success = false, message = "No valid verification request found." });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resending OTP for email: {Email}", Email);
            return new JsonResult(new { success = false, message = ex.Message });
        }
    }

    private async Task<IdentityResult> CompleteRegistrationAsync(RegistrationData data)
    {
        // Create ApplicationUser
        var user = new ApplicationUser
        {
            UserName = data.Email,
            Email = data.Email,
            FirstName = data.FirstName,
            LastName = data.LastName,
            EmailConfirmed = true,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, data.Password);

        if (result.Succeeded)
        {
            _logger.LogInformation("User created a new account with password.");

            // Assign "Member" role to the new user
            await _userManager.AddToRoleAsync(user, "Member");

            // Handle file upload if any
            string? visaIdFilePath = null;
            if (!string.IsNullOrEmpty(data.VisaIdFileContent) && !string.IsNullOrEmpty(data.VisaIdFileName))
            {
                visaIdFilePath = await SaveVisaIdFileFromBase64(data.VisaIdFileContent, data.VisaIdFileName, data.MemberId);
            }

            // Create Member record
            var member = new Member
            {
                MemberId = data.MemberId,
                ApplicationUserId = user.Id,
                FirstName = data.FirstName,
                MiddleName = data.MiddleName,
                LastName = data.LastName,
                DateOfBirth = data.DateOfBirth.HasValue ? DateTime.SpecifyKind(data.DateOfBirth.Value, DateTimeKind.Utc) : null,
                Gender = data.Gender,
                Nationality = data.Nationality,
                EmailAddress = data.Email,
                PhoneNumber = data.PhoneNumber,
                Address = data.Address,
                Emirate = data.Emirate,
                City = data.City,
                PassportNumber = data.PassportNumber,
                EmiratesId = data.EmiratesId,
                VisaType = data.VisaType,
                VisaIdFilePath = visaIdFilePath,
                EmploymentStatus = data.EmploymentStatus,
                CompanyName = data.CompanyName,
                DoYouKnowAboutTAE = data.DoYouKnowAboutTAE,
                Advice = data.Advice,
                OptInNidaService = data.OptInNidaService,
                CreatedAt = DateTime.UtcNow,
                NidaServiceStatus = NidaServiceStatus.Completed
            };

            _context.Members.Add(member);
            await _context.SaveChangesAsync();

            // Sign in the user
            await _signInManager.SignInAsync(user, isPersistent: false);
        }

        return result;
    }

    private async Task<string> SaveVisaIdFile(IFormFile file, string memberId)
    {
        if (file.Length == 0) return string.Empty;

        var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "visa-ids", DateTime.Now.Year.ToString());
        Directory.CreateDirectory(uploadsFolder);

        var fileName = $"{memberId}_{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        var filePath = Path.Combine(uploadsFolder, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        return Path.Combine("uploads", "visa-ids", DateTime.Now.Year.ToString(), fileName);
    }

    private async Task<string> SaveVisaIdFileFromBase64(string base64Content, string fileName, string memberId)
    {
        var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "visa-ids", DateTime.Now.Year.ToString());
        Directory.CreateDirectory(uploadsFolder);

        var fileExtension = Path.GetExtension(fileName);
        var newFileName = $"{memberId}_{Guid.NewGuid()}{fileExtension}";
        var filePath = Path.Combine(uploadsFolder, newFileName);

        var fileBytes = Convert.FromBase64String(base64Content);
        await System.IO.File.WriteAllBytesAsync(filePath, fileBytes);

        return Path.Combine("uploads", "visa-ids", DateTime.Now.Year.ToString(), newFileName);
    }

    // Helper class to store registration data temporarily
    public class RegistrationData
    {
        public string FirstName { get; set; } = string.Empty;
        public string? MiddleName { get; set; }
        public string LastName { get; set; } = string.Empty;
        public DateTime? DateOfBirth { get; set; }
        public string Gender { get; set; } = string.Empty;
        public string Nationality { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Emirate { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string? PassportNumber { get; set; }
        public string? EmiratesId { get; set; }
        public string VisaType { get; set; } = string.Empty;
        public string? EmploymentStatus { get; set; }
        public string? CompanyName { get; set; }
        public bool DoYouKnowAboutTAE { get; set; }
        public string? Advice { get; set; }
        public bool OptInNidaService { get; set; }
        public string Password { get; set; } = string.Empty;
        public string MemberId { get; set; } = string.Empty;
        public string? VisaIdFileName { get; set; }
        public string? VisaIdFileContent { get; set; }
    }
}
