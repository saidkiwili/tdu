using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using tae_app.Data;
using tae_app.Models;

namespace tae_app.Controllers;

public class RegistrationController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<RegistrationController> _logger;

    public RegistrationController(ApplicationDbContext context, ILogger<RegistrationController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Index()
    {
        return View(new Member());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(Member model)
    {
        if (ModelState.IsValid)
        {
            try
            {
                // Generate unique MemberID
                model.MemberId = GenerateUniqueMemberId();
                
                // Handle file upload if provided
                if (model.VisaIdFile != null && model.VisaIdFile.Length > 0)
                {
                    model.VisaIdFilePath = await SaveVisaFile(model.VisaIdFile);
                }

                // Set creation timestamp
                model.CreatedAt = DateTime.UtcNow;

                // Add member to database
                _context.Members.Add(model);
                await _context.SaveChangesAsync();

                // If NIDA service opted in, handle payment flow
                if (model.OptInNidaService)
                {
                    return RedirectToAction("NidaPayment", new { memberId = model.Id });
                }

                TempData["Success"] = $"Registration successful! Your Member ID is: {model.MemberId}";
                return RedirectToAction("Success", new { memberId = model.MemberId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during member registration");
                ModelState.AddModelError("", "An error occurred during registration. Please try again.");
            }
        }

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> NidaPayment(int memberId)
    {
        var member = await _context.Members.FindAsync(memberId);
        if (member == null)
        {
            return NotFound();
        }

        ViewBag.Member = member;
        ViewBag.NidaServiceFee = 50.00m; // AED 50 example fee
        
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ProcessNidaPayment(int memberId, string paymentMethod)
    {
        var member = await _context.Members.FindAsync(memberId);
        if (member == null)
        {
            return NotFound();
        }

        try
        {
            // Simulate payment processing
            var paymentReference = Guid.NewGuid().ToString("N")[..10].ToUpper();
            
            member.PaymentReference = paymentReference;
            member.AmountPaid = 50.00m;
            member.PaymentMethod = paymentMethod;
            member.NidaServiceStatus = NidaServiceStatus.Paid;

            await _context.SaveChangesAsync();

            // Create appointment
            var appointment = new Appointment
            {
                MemberId = member.Id,
                ServiceType = "NIDA Service",
                ScheduledAt = DateTime.UtcNow.AddDays(7), // 1 week from now
                Location = "TAE Office, Dubai",
                Status = AppointmentStatus.Scheduled,
                Notes = $"Payment Reference: {paymentReference}"
            };

            _context.Appointments.Add(appointment);
            member.NidaServiceStatus = NidaServiceStatus.AppointmentScheduled;
            
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Payment successful! Your appointment is scheduled for {appointment.ScheduledAt:MMM dd, yyyy} at {appointment.ScheduledAt:HH:mm}";
            return RedirectToAction("AppointmentConfirmation", new { appointmentId = appointment.Id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing NIDA payment for member {MemberId}", memberId);
            TempData["Error"] = "Payment processing failed. Please try again.";
            return RedirectToAction("NidaPayment", new { memberId });
        }
    }

    [HttpGet]
    public async Task<IActionResult> AppointmentConfirmation(int appointmentId)
    {
        var appointment = await _context.Appointments
            .Include(a => a.Member)
            .FirstOrDefaultAsync(a => a.Id == appointmentId);

        if (appointment == null)
        {
            return NotFound();
        }

        return View(appointment);
    }

    [HttpGet]
    public IActionResult Success(string memberId)
    {
        ViewBag.MemberId = memberId;
        return View();
    }

    private string GenerateUniqueMemberId()
    {
        var year = DateTime.UtcNow.Year;
        var random = new Random();
        string memberId;
        
        do
        {
            var number = random.Next(1000, 9999);
            memberId = $"TAE-{year}-{number}";
        } 
        while (_context.Members.Any(m => m.MemberId == memberId));

        return memberId;
    }

    private async Task<string> SaveVisaFile(IFormFile file)
    {
        var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "visa");
        Directory.CreateDirectory(uploadsPath);

        var fileName = $"{Guid.NewGuid()}_{file.FileName}";
        var filePath = Path.Combine(uploadsPath, fileName);

        using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream);

        return $"/uploads/visa/{fileName}";
    }
}
