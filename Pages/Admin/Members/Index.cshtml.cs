using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Text.Json;
using tae_app.Data;
using tae_app.Models;

namespace tae_app.Pages.Admin.Members
{
    [Authorize(Roles = "SuperAdmin,Admin,MemberEditor,MemberViewer")]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        // Properties for data binding
        public List<Member> Members { get; set; } = new();
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalMembers { get; set; }
        public int TotalPages { get; set; }
        public string SearchTerm { get; set; } = string.Empty;
        public string StatusFilter { get; set; } = string.Empty;
        public string MembershipTypeFilter { get; set; } = string.Empty;
        public string SortBy { get; set; } = "CreatedAt";
        public string SortOrder { get; set; } = "desc";

        public async Task OnGetAsync(int page = 1, string search = "", 
            string status = "", string membershipType = "", string sortBy = "CreatedAt", 
            string sortOrder = "desc", int pageSize = 10)
        {
            CurrentPage = Math.Max(1, page);
            SearchTerm = search ?? string.Empty;
            StatusFilter = status ?? string.Empty;
            MembershipTypeFilter = membershipType ?? string.Empty;
            SortBy = sortBy ?? "CreatedAt";
            SortOrder = sortOrder ?? "desc";
            PageSize = pageSize > 0 ? pageSize : 10;

            var query = _context.Members
                .Include(m => m.ApplicationUser)
                .AsQueryable();

            // Apply search filter
            if (!string.IsNullOrEmpty(SearchTerm))
            {
                query = query.Where(m => 
                    m.FirstName.Contains(SearchTerm) ||
                    m.LastName.Contains(SearchTerm) ||
                    m.EmailAddress.Contains(SearchTerm) ||
                    m.PhoneNumber.Contains(SearchTerm) ||
                    m.MemberId.Contains(SearchTerm));
            }

            // Apply status filter (using NIDA service status as proxy for member status)
            if (!string.IsNullOrEmpty(StatusFilter))
            {
                switch (StatusFilter.ToLower())
                {
                    case "pending":
                        query = query.Where(m => m.NidaServiceStatus == NidaServiceStatus.None || 
                                               m.NidaServiceStatus == NidaServiceStatus.PendingPayment);
                        break;
                    case "approved":
                        query = query.Where(m => m.NidaServiceStatus == NidaServiceStatus.Completed);
                        break;
                    case "active":
                        query = query.Where(m => m.NidaServiceStatus == NidaServiceStatus.Paid ||
                                               m.NidaServiceStatus == NidaServiceStatus.AppointmentScheduled ||
                                               m.NidaServiceStatus == NidaServiceStatus.Completed);
                        break;
                    case "blocked":
                        query = query.Where(m => m.ApplicationUser != null && !m.ApplicationUser.IsActive);
                        break;
                }
            }

            // Apply membership type filter (using employment status as proxy)
            if (!string.IsNullOrEmpty(MembershipTypeFilter))
            {
                switch (MembershipTypeFilter.ToLower())
                {
                    case "individual":
                        query = query.Where(m => string.IsNullOrEmpty(m.CompanyName));
                        break;
                    case "corporate":
                        query = query.Where(m => !string.IsNullOrEmpty(m.CompanyName));
                        break;
                    case "student":
                        query = query.Where(m => m.EmploymentStatus != null && 
                                               m.EmploymentStatus.ToLower().Contains("student"));
                        break;
                }
            }

            // Apply sorting
            query = SortBy.ToLower() switch
            {
                "name" => SortOrder == "asc" ? 
                    query.OrderBy(m => m.FirstName).ThenBy(m => m.LastName) :
                    query.OrderByDescending(m => m.FirstName).ThenByDescending(m => m.LastName),
                "email" => SortOrder == "asc" ? 
                    query.OrderBy(m => m.EmailAddress) : query.OrderByDescending(m => m.EmailAddress),
                "phone" => SortOrder == "asc" ? 
                    query.OrderBy(m => m.PhoneNumber) : query.OrderByDescending(m => m.PhoneNumber),
                "createdat" => SortOrder == "asc" ? 
                    query.OrderBy(m => m.CreatedAt) : query.OrderByDescending(m => m.CreatedAt),
                _ => query.OrderByDescending(m => m.CreatedAt)
            };

            // Get total count for pagination
            TotalMembers = await query.CountAsync();
            TotalPages = (int)Math.Ceiling((double)TotalMembers / PageSize);

            // Apply pagination
            Members = await query
                .Skip((CurrentPage - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            if (!User.IsInRole("SuperAdmin") && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            var member = await _context.Members
                .Include(m => m.Appointments)
                .Include(m => m.EventRegistrations)
                .Include(m => m.JobApplications)
                .FirstOrDefaultAsync(m => m.Id == id);
                
            if (member != null)
            {
                // Remove related records first
                if (member.Appointments.Any())
                {
                    _context.Appointments.RemoveRange(member.Appointments);
                }
                
                if (member.EventRegistrations.Any())
                {
                    _context.EventRegistrations.RemoveRange(member.EventRegistrations);
                }
                
                if (member.JobApplications.Any())
                {
                    _context.JobApplications.RemoveRange(member.JobApplications);
                }
                
                // Remove the member
                _context.Members.Remove(member);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Member and all related records deleted successfully.";
            }
            else
            {
                TempData["Error"] = "Member not found.";
            }

            return RedirectToPage("./Index");
        }

        public async Task<IActionResult> OnPostApproveAsync(int id)
        {
            var member = await _context.Members.FindAsync(id);
            if (member != null)
            {
                member.NidaServiceStatus = NidaServiceStatus.Completed;
                await _context.SaveChangesAsync();
                TempData["Success"] = "Member approved successfully.";
            }

            return RedirectToPage("./Index");
        }

        public async Task<IActionResult> OnPostRejectAsync(int id)
        {
            var member = await _context.Members.FindAsync(id);
            if (member != null)
            {
                member.NidaServiceStatus = NidaServiceStatus.None;
                await _context.SaveChangesAsync();
                TempData["Success"] = "Member status updated successfully.";
            }

            return RedirectToPage("./Index");
        }

        public async Task<IActionResult> OnPostBlockAsync(int id)
        {
            if (!User.IsInRole("SuperAdmin") && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            var member = await _context.Members
                .Include(m => m.ApplicationUser)
                .FirstOrDefaultAsync(m => m.Id == id);
                
            if (member?.ApplicationUser != null)
            {
                member.ApplicationUser.IsActive = false;
                await _context.SaveChangesAsync();
                TempData["Success"] = "Member blocked successfully.";
            }
            else
            {
                TempData["Error"] = "Member not found or not linked to user account.";
            }

            return RedirectToPage("./Index");
        }

        public async Task<IActionResult> OnPostUnblockAsync(int id)
        {
            if (!User.IsInRole("SuperAdmin") && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            var member = await _context.Members
                .Include(m => m.ApplicationUser)
                .FirstOrDefaultAsync(m => m.Id == id);
                
            if (member?.ApplicationUser != null)
            {
                member.ApplicationUser.IsActive = true;
                await _context.SaveChangesAsync();
                TempData["Success"] = "Member unblocked successfully.";
            }
            else
            {
                TempData["Error"] = "Member not found or not linked to user account.";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnGetExportAsync(string format = "csv")
        {
            var query = _context.Members
                .Include(m => m.ApplicationUser)
                .AsQueryable();

            // Apply same filters as the current view
            if (!string.IsNullOrEmpty(SearchTerm))
            {
                query = query.Where(m => 
                    m.FirstName.Contains(SearchTerm) ||
                    m.LastName.Contains(SearchTerm) ||
                    m.EmailAddress.Contains(SearchTerm) ||
                    m.PhoneNumber.Contains(SearchTerm) ||
                    m.MemberId.Contains(SearchTerm));
            }

            var members = await query.OrderByDescending(m => m.CreatedAt).ToListAsync();

            if (format.ToLower() == "json")
            {
                var jsonData = JsonSerializer.Serialize(members.Select(m => new
                {
                    m.MemberId,
                    FullName = m.FullName,
                    m.EmailAddress,
                    m.PhoneNumber,
                    m.Nationality,
                    m.Emirate,
                    m.EmploymentStatus,
                    m.CompanyName,
                    Status = GetMemberStatus(m),
                    m.CreatedAt
                }), new JsonSerializerOptions { WriteIndented = true });

                return File(Encoding.UTF8.GetBytes(jsonData), "application/json", 
                    $"members_export_{DateTime.Now:yyyyMMdd_HHmmss}.json");
            }
            else if (format.ToLower() == "xlsx" || format.ToLower() == "xls")
            {
                // Create Excel using ClosedXML
                using var workbook = new ClosedXML.Excel.XLWorkbook();
                var ws = workbook.Worksheets.Add("Members");
                var headers = new[] { "Member ID", "Full Name", "Email", "Phone", "Nationality", "Emirate", "Employment Status", "Company", "Status", "Registration Date" };
                for (int i = 0; i < headers.Length; i++)
                {
                    ws.Cell(1, i + 1).Value = headers[i];
                }

                int row = 2;
                foreach (var member in members)
                {
                    ws.Cell(row, 1).Value = member.MemberId;
                    ws.Cell(row, 2).Value = member.FullName;
                    ws.Cell(row, 3).Value = member.EmailAddress;
                    ws.Cell(row, 4).Value = member.PhoneNumber;
                    ws.Cell(row, 5).Value = member.Nationality;
                    ws.Cell(row, 6).Value = member.Emirate;
                    ws.Cell(row, 7).Value = member.EmploymentStatus;
                    ws.Cell(row, 8).Value = member.CompanyName;
                    ws.Cell(row, 9).Value = GetMemberStatus(member);
                    ws.Cell(row, 10).Value = member.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss");
                    row++;
                }

                using var ms = new System.IO.MemoryStream();
                workbook.SaveAs(ms);
                ms.Position = 0;
                return File(ms.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", 
                    $"members_export_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
            }
            else
            {
                // CSV export
                var csv = new StringBuilder();
                csv.AppendLine("Member ID,Full Name,Email,Phone,Nationality,Emirate,Employment Status,Company,Status,Registration Date");

                foreach (var member in members)
                {
                    csv.AppendLine($"{member.MemberId}," +
                                 $"\"{member.FullName}\"," +
                                 $"{member.EmailAddress}," +
                                 $"{member.PhoneNumber}," +
                                 $"{member.Nationality}," +
                                 $"{member.Emirate}," +
                                 $"\"{member.EmploymentStatus}\"," +
                                 $"\"{member.CompanyName}\"," +
                                 $"{GetMemberStatus(member)}," +
                                 $"{member.CreatedAt:yyyy-MM-dd HH:mm:ss}");
                }

                return File(Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", 
                    $"members_export_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
            }
        }

        public async Task<IActionResult> OnGetViewAsync(int id)
        {
            var member = await _context.Members
                .Include(m => m.ApplicationUser)
                .FirstOrDefaultAsync(m => m.Id == id);
                
            if (member == null)
            {
                return NotFound();
            }

            return new JsonResult(new
            {
                member.Id,
                member.MemberId,
                FullName = member.FullName,
                member.EmailAddress,
                member.PhoneNumber,
                member.DateOfBirth,
                member.Gender,
                member.Nationality,
                member.PassportNumber,
                member.EmiratesId,
                member.Address,
                member.City,
                member.Emirate,
                member.EmploymentStatus,
                member.CompanyName,
                member.VisaType,
                member.VisaIdFilePath,
                member.DoYouKnowAboutTAE,
                member.Advice,
                member.OptInNidaService,
                NidaServiceStatus = member.NidaServiceStatus.ToString(),
                member.PaymentReference,
                member.AmountPaid,
                member.PaymentMethod,
                Status = GetMemberStatus(member),
                member.CreatedAt
            });
        }

        private static string GetMemberStatus(Member member)
        {
            // Check if user account is blocked
            if (member.ApplicationUser != null && !member.ApplicationUser.IsActive)
                return "Blocked";
                
            return member.NidaServiceStatus switch
            {
                NidaServiceStatus.None => "Pending",
                NidaServiceStatus.PendingPayment => "Pending Payment",
                NidaServiceStatus.Paid => "Active",
                NidaServiceStatus.AppointmentScheduled => "Active",
                NidaServiceStatus.Completed => "Approved",
                _ => "Unknown"
            };
        }

        private static string GetMembershipType(Member member)
        {
            if (!string.IsNullOrEmpty(member.CompanyName))
                return "Corporate";
            
            if (member.EmploymentStatus?.ToLower().Contains("student") == true)
                return "Student";
                
            return "Individual";
        }
    }
}
