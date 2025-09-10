using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using tae_app.Data;
using tae_app.Models;

namespace tae_app.Controllers;

[Authorize(Roles = "SuperAdmin,Admin")]
public class AdminController : Controller
{
    private readonly ApplicationDbContext _context;

    public AdminController(ApplicationDbContext context)
    {
        _context = context;
    }

    public IActionResult Index()
    {
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> Members(int page = 1, int pageSize = 20)
    {
        var membersQuery = _context.Members
            .Include(m => m.Appointments)
            .OrderByDescending(m => m.CreatedAt);

        var totalMembers = await membersQuery.CountAsync();
        var members = await membersQuery
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        ViewBag.CurrentPage = page;
        ViewBag.PageSize = pageSize;
        ViewBag.TotalMembers = totalMembers;
        ViewBag.TotalPages = (int)Math.Ceiling((double)totalMembers / pageSize);

        return View(members);
    }

    [HttpGet]
    public async Task<IActionResult> MemberDetails(int id)
    {
        var member = await _context.Members
            .Include(m => m.Appointments)
            .Include(m => m.EventRegistrations)
            .Include(m => m.JobApplications)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (member == null)
        {
            return NotFound();
        }

        return View(member);
    }

    [HttpGet]
    public async Task<IActionResult> Appointments(int page = 1, int pageSize = 20)
    {
        var appointmentsQuery = _context.Appointments
            .Include(a => a.Member)
            .OrderByDescending(a => a.ScheduledAt);

        var totalAppointments = await appointmentsQuery.CountAsync();
        var appointments = await appointmentsQuery
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        ViewBag.CurrentPage = page;
        ViewBag.PageSize = pageSize;
        ViewBag.TotalAppointments = totalAppointments;
        ViewBag.TotalPages = (int)Math.Ceiling((double)totalAppointments / pageSize);

        return View(appointments);
    }

    [HttpPost]
    public async Task<IActionResult> UpdateAppointmentStatus(int id, AppointmentStatus status, string? notes)
    {
        var appointment = await _context.Appointments.FindAsync(id);
        if (appointment == null)
        {
            return NotFound();
        }

        appointment.Status = status;
        appointment.Notes = notes;
        appointment.UpdatedAt = DateTime.UtcNow;

        // If appointment is completed, update member's NIDA status
        if (status == AppointmentStatus.Completed)
        {
            var member = await _context.Members.FindAsync(appointment.MemberId);
            if (member != null)
            {
                member.NidaServiceStatus = NidaServiceStatus.Completed;
            }
        }

        await _context.SaveChangesAsync();

        TempData["Success"] = "Appointment status updated successfully.";
        return RedirectToAction("Appointments");
    }

    [HttpGet]
    public async Task<IActionResult> Dashboard()
    {
        var stats = new
        {
            TotalMembers = await _context.Members.CountAsync(),
            NewMembersThisMonth = await _context.Members
                .Where(m => m.CreatedAt >= DateTime.UtcNow.AddDays(-30))
                .CountAsync(),
            NidaServiceRequests = await _context.Members
                .Where(m => m.OptInNidaService)
                .CountAsync(),
            PendingAppointments = await _context.Appointments
                .Where(a => a.Status == AppointmentStatus.Scheduled)
                .CountAsync(),
            CompletedAppointments = await _context.Appointments
                .Where(a => a.Status == AppointmentStatus.Completed)
                .CountAsync(),
            TotalRevenue = await _context.Members
                .Where(m => m.AmountPaid.HasValue)
                .SumAsync(m => m.AmountPaid ?? 0)
        };

        return View(stats);
    }

    [HttpGet]
    public async Task<IActionResult> ExportMembers()
    {
        var members = await _context.Members
            .OrderBy(m => m.LastName)
            .ThenBy(m => m.FirstName)
            .ToListAsync();

        var csv = "MemberID,FirstName,LastName,Email,Phone,City,Emirate,EmploymentStatus,OptInNida,CreatedAt\n";
        
        foreach (var member in members)
        {
            csv += $"{member.MemberId},{member.FirstName},{member.LastName},{member.EmailAddress},{member.PhoneNumber},{member.City},{member.Emirate},{member.EmploymentStatus},{member.OptInNidaService},{member.CreatedAt:yyyy-MM-dd}\n";
        }

        var bytes = System.Text.Encoding.UTF8.GetBytes(csv);
        return File(bytes, "text/csv", $"tae_members_{DateTime.UtcNow:yyyyMMdd}.csv");
    }
}
