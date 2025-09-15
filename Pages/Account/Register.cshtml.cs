using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using tae_app.Models;
using tae_app.Data;
using tae_app.Services;

namespace tae_app.Pages.Account;

public class RegisterModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<RegisterModel> _logger;

    public RegisterModel(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ApplicationDbContext context,
        IWebHostEnvironment environment,
        ILogger<RegisterModel> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _context = context;
        _environment = environment;
        _logger = logger;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public List<CountryData> Countries { get; set; } = new();
    public List<EmirateData> EmiratesCities { get; set; } = new();
    public List<string> EmploymentStatuses { get; set; } = new();
    public List<string> VisaTypes { get; set; } = new();

    public class InputModel
    {
        // Personal Information
        [Required]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        [Display(Name = "Middle Name")]
        public string? MiddleName { get; set; }

        [Required]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;

        [Display(Name = "Date of Birth")]
        [DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; }

        [Required]
        [Display(Name = "Gender")]
        public string Gender { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Nationality")]
        public string Nationality { get; set; } = string.Empty;

        // Contact Information
        [Required]
        [EmailAddress]
        [Display(Name = "Email Address")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [Phone]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; } = string.Empty;

        // Address Information
        [Required]
        [Display(Name = "Address")]
        public string Address { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Emirate")]
        public string Emirate { get; set; } = string.Empty;

        [Required]
        [Display(Name = "City")]
        public string City { get; set; } = string.Empty;

        // Identity Documents
        [Display(Name = "Passport Number")]
        public string? PassportNumber { get; set; }

        [Display(Name = "Emirates ID")]
        public string? EmiratesId { get; set; }

        [Required]
        [Display(Name = "Visa Type")]
        public string VisaType { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Visa ID File")]
        public IFormFile VisaIdFile { get; set; } = null!;

        // Employment Information
        [Display(Name = "Employment Status")]
        public string? EmploymentStatus { get; set; }

        [Display(Name = "Company Name")]
        public string? CompanyName { get; set; }

        // TDUAE Information
        [Display(Name = "Do you know about TDUAE?")]
        public bool DoYouKnowAboutTAE { get; set; }

        [Display(Name = "Any advice or comments?")]
        public string? Advice { get; set; }

        // NIDA Service
        [Display(Name = "Opt-in for NIDA Services")]
        public bool OptInNidaService { get; set; }

        // Account Creation
        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public class CountryData
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Flag { get; set; } = string.Empty;
    }

    public class EmirateData
    {
        public string Emirate { get; set; } = string.Empty;
        public List<string> Cities { get; set; } = new();
    }

    public async Task OnGetAsync()
    {
        await LoadStaticData();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await LoadStaticData();

        if (!ModelState.IsValid)
        {
            return Page();
        }

        // Validate uniqueness constraints
        var existingMemberByEmail = await _context.Members
            .FirstOrDefaultAsync(m => m.EmailAddress.ToLower() == Input.Email.ToLower());
        if (existingMemberByEmail != null)
        {
            ModelState.AddModelError("Input.Email", "An account with this email address already exists.");
            return Page();
        }

        var existingMemberByPhone = await _context.Members
            .FirstOrDefaultAsync(m => m.PhoneNumber == Input.PhoneNumber);
        if (existingMemberByPhone != null)
        {
            ModelState.AddModelError("Input.PhoneNumber", "An account with this phone number already exists.");
            return Page();
        }

        if (!string.IsNullOrEmpty(Input.EmiratesId))
        {
            var existingMemberByEmiratesId = await _context.Members
                .FirstOrDefaultAsync(m => m.EmiratesId == Input.EmiratesId);
            if (existingMemberByEmiratesId != null)
            {
                ModelState.AddModelError("Input.EmiratesId", "An account with this Emirates ID already exists.");
                return Page();
            }
        }

        try
        {
            // Generate Member ID
            var year = DateTime.Now.Year;
            var count = await _context.Members.CountAsync() + 1;
            var memberId = $"TAE-{year}-{count:D4}";

            // Prepare registration data for temporary storage
            var registrationData = new
            {
                FirstName = Input.FirstName,
                MiddleName = Input.MiddleName,
                LastName = Input.LastName,
                DateOfBirth = Input.DateOfBirth,
                Gender = Input.Gender,
                Nationality = Input.Nationality,
                Email = Input.Email,
                PhoneNumber = Input.PhoneNumber,
                Address = Input.Address,
                Emirate = Input.Emirate,
                City = Input.City,
                PassportNumber = Input.PassportNumber,
                EmiratesId = Input.EmiratesId,
                VisaType = Input.VisaType,
                EmploymentStatus = Input.EmploymentStatus,
                CompanyName = Input.CompanyName,
                DoYouKnowAboutTAE = Input.DoYouKnowAboutTAE,
                Advice = Input.Advice,
                OptInNidaService = Input.OptInNidaService,
                Password = Input.Password,
                MemberId = memberId,
                VisaIdFileName = Input.VisaIdFile?.FileName,
                VisaIdFileContent = Input.VisaIdFile != null ? await ConvertFileToBase64(Input.VisaIdFile) : null
            };

            // Serialize registration data
            var registrationJson = System.Text.Json.JsonSerializer.Serialize(registrationData);

            // Generate and send OTP
            var otpService = HttpContext.RequestServices.GetRequiredService<IOtpService>();
            var otp = await otpService.GenerateOtpAsync(Input.Email);

            // Store registration data in OTP record
            var otpRecord = await _context.OtpVerifications
                .FirstOrDefaultAsync(o => o.Email == Input.Email && !o.IsUsed);

            if (otpRecord != null)
            {
                otpRecord.RegistrationData = registrationJson;
                await _context.SaveChangesAsync();
            }

            // Send OTP email
            await otpService.SendOtpEmailAsync(Input.Email, otp);

            _logger.LogInformation("OTP sent to email: {Email} for registration", Input.Email);

            // Don't show success message here - show it after OTP verification
            return RedirectToPage("/Account/VerifyOtp", new { email = Input.Email });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending OTP for email: {Email}", Input.Email);
            ModelState.AddModelError("", "An error occurred while sending the verification code. Please try again.");
            return Page();
        }
    }

    private async Task LoadStaticData()
    {
        try
        {
            var contentRoot = _environment.ContentRootPath;

            // Load Countries
            var countriesJson = await System.IO.File.ReadAllTextAsync(
                Path.Combine(contentRoot, "Data", "StaticData", "countries.json"));
            Countries = JsonSerializer.Deserialize<List<CountryData>>(countriesJson) ?? new();

            // Load Emirates and Cities
            var emiratesJson = await System.IO.File.ReadAllTextAsync(
                Path.Combine(contentRoot, "Data", "StaticData", "emirates-cities.json"));
            EmiratesCities = JsonSerializer.Deserialize<List<EmirateData>>(emiratesJson) ?? new();

            // Load Employment Status
            var employmentJson = await System.IO.File.ReadAllTextAsync(
                Path.Combine(contentRoot, "Data", "StaticData", "employment-status.json"));
            EmploymentStatuses = JsonSerializer.Deserialize<List<string>>(employmentJson) ?? new();

            // Load Visa Types
            var visaTypesJson = await System.IO.File.ReadAllTextAsync(
                Path.Combine(contentRoot, "Data", "StaticData", "visa-types.json"));
            VisaTypes = JsonSerializer.Deserialize<List<string>>(visaTypesJson) ?? new();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading static data");
            // Initialize empty lists as fallback
            Countries = new();
            EmiratesCities = new();
            EmploymentStatuses = new();
            VisaTypes = new();
        }
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

    private async Task<string> ConvertFileToBase64(IFormFile file)
    {
        using (var memoryStream = new MemoryStream())
        {
            await file.CopyToAsync(memoryStream);
            var fileBytes = memoryStream.ToArray();
            return Convert.ToBase64String(fileBytes);
        }
    }
}
