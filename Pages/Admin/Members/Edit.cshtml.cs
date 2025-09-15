using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using tae_app.Data;
using tae_app.Models;

namespace tae_app.Pages.Admin.Members
{
    [Authorize(Roles = "SuperAdmin,Admin,MemberEditor")]
    public class EditModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public EditModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Member Member { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var member = await _context.Members.FirstOrDefaultAsync(m => m.Id == id);

            if (member == null)
            {
                TempData["Error"] = "Member not found.";
                return RedirectToPage("/Admin/Members");
            }

            Member = member;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Check if member exists
            var existingMember = await _context.Members.AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == Member.Id);

            if (existingMember == null)
            {
                TempData["Error"] = "Member not found.";
                return RedirectToPage("/Admin/Members");
            }

            // Check for duplicate email (excluding current member)
            var duplicateEmail = await _context.Members
                .AnyAsync(m => m.EmailAddress == Member.EmailAddress && m.Id != Member.Id);

            if (duplicateEmail)
            {
                ModelState.AddModelError("Member.EmailAddress", "Email address is already in use by another member.");
                return Page();
            }

            // Check for duplicate phone number (excluding current member)
            var duplicatePhone = await _context.Members
                .AnyAsync(m => m.PhoneNumber == Member.PhoneNumber && m.Id != Member.Id);

            if (duplicatePhone)
            {
                ModelState.AddModelError("Member.PhoneNumber", "Phone number is already in use by another member.");
                return Page();
            }

            // Check for duplicate emirates ID (excluding current member)
            if (!string.IsNullOrEmpty(Member.EmiratesId))
            {
                var duplicateEmiratesId = await _context.Members
                    .AnyAsync(m => m.EmiratesId == Member.EmiratesId && m.Id != Member.Id);

                if (duplicateEmiratesId)
                {
                    ModelState.AddModelError("Member.EmiratesId", "Emirates ID is already in use by another member.");
                    return Page();
                }
            }

            try
            {
                // Preserve original values that shouldn't be modified
                Member.MemberId = existingMember.MemberId;
                // Ensure CreatedAt has UTC kind (Npgsql requires UTC for timestamptz)
                Member.CreatedAt = existingMember.CreatedAt.Kind == DateTimeKind.Utc
                    ? existingMember.CreatedAt
                    : DateTime.SpecifyKind(existingMember.CreatedAt, DateTimeKind.Utc);
                Member.ApplicationUserId = existingMember.ApplicationUserId;
                Member.VisaIdFilePath = existingMember.VisaIdFilePath;
                Member.PaymentReference = existingMember.PaymentReference;
                Member.AmountPaid = existingMember.AmountPaid;
                Member.PaymentMethod = existingMember.PaymentMethod;

                // Normalize any DateTime properties coming from the form to UTC kind
                if (Member.DateOfBirth.HasValue)
                {
                    var dob = Member.DateOfBirth.Value;
                    if (dob.Kind == DateTimeKind.Local)
                    {
                        Member.DateOfBirth = dob.ToUniversalTime();
                    }
                    else if (dob.Kind == DateTimeKind.Unspecified)
                    {
                        // Treat unspecified as UTC at the same tick value
                        Member.DateOfBirth = DateTime.SpecifyKind(dob, DateTimeKind.Utc);
                    }
                }

                _context.Attach(Member).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                TempData["Success"] = "Member updated successfully.";
                return RedirectToPage("./Index");
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MemberExists(Member.Id))
                {
                    TempData["Error"] = "Member not found.";
                    return RedirectToPage("./Index");
                }
                else
                {
                    throw;
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "An error occurred while updating the member: " + ex.Message);
                return Page();
            }
        }

        private bool MemberExists(int id)
        {
            return _context.Members.Any(e => e.Id == id);
        }
    }
}
