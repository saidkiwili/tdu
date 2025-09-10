using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using tae_app.Models;

namespace tae_app.Data;

public static class SeedData
{
    public static async Task Initialize(IServiceProvider serviceProvider)
    {
        using var context = new ApplicationDbContext(
            serviceProvider.GetRequiredService<DbContextOptions<ApplicationDbContext>>());

        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        // Ensure database is created
        await context.Database.EnsureCreatedAsync();

        // Seed roles
        await SeedRoles(roleManager);

        // Seed admin user
        await SeedAdminUser(userManager);

        // Seed sample members for testing
        await SeedMembers(context);

        await context.SaveChangesAsync();
    }

    private static async Task SeedRoles(RoleManager<IdentityRole> roleManager)
    {
        string[] roles = { "SuperAdmin", "Admin", "MemberEditor", "MemberViewer", "EventManager" };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }
    }

    private static async Task SeedAdminUser(UserManager<ApplicationUser> userManager)
    {
        var adminEmail = "admin@tae.ae";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);

        if (adminUser == null)
        {
            adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true,
                FirstName = "TAE",
                LastName = "Administrator",
                IsActive = true
            };

            var result = await userManager.CreateAsync(adminUser, "TaeAdmin123!");

            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "SuperAdmin");
            }
        }
    }

    private static async Task SeedMembers(ApplicationDbContext context)
    {
        // Check if members already exist
        if (await context.Members.AnyAsync())
        {
            return;
        }

        var members = new[]
        {
            new Member
            {
                MemberId = GenerateMemberId(DateTime.UtcNow.AddDays(-30)),
                FirstName = "Ahmed",
                LastName = "Al-Mansouri",
                EmailAddress = "ahmed.almansouri@email.com",
                PhoneNumber = "+971 50 123 4567",
                DateOfBirth = new DateTime(1985, 3, 15),
                Gender = "Male",
                Nationality = "Emirati",
                PassportNumber = "UAE123456789",
                EmiratesId = "784-1985-1234567-1",
                Address = "Al Wasl Road, Villa 123",
                City = "Dubai",
                Emirate = "Dubai",
                EmploymentStatus = "Employed",
                CompanyName = "Emirates Airlines",
                VisaType = "Resident",
                DoYouKnowAboutTAE = true,
                OptInNidaService = true,
                NidaServiceStatus = NidaServiceStatus.Completed,
                Advice = "Looking forward to networking events and business development opportunities.",
                CreatedAt = DateTime.UtcNow.AddDays(-30)
            },
            new Member
            {
                MemberId = GenerateMemberId(DateTime.UtcNow.AddDays(-25)),
                FirstName = "Sarah",
                LastName = "Johnson",
                EmailAddress = "sarah.johnson@email.com",
                PhoneNumber = "+971 55 987 6543",
                DateOfBirth = new DateTime(1992, 8, 22),
                Gender = "Female",
                Nationality = "American",
                PassportNumber = "USA987654321",
                EmiratesId = "784-1992-9876543-2",
                Address = "Business Bay, Tower A, Apt 1502",
                City = "Dubai",
                Emirate = "Dubai",
                EmploymentStatus = "Self-employed",
                CompanyName = "Johnson Consulting LLC",
                VisaType = "Investor",
                DoYouKnowAboutTAE = false,
                OptInNidaService = true,
                NidaServiceStatus = NidaServiceStatus.Paid,
                Advice = "Interested in investment opportunities and business partnerships.",
                CreatedAt = DateTime.UtcNow.AddDays(-25)
            },
            new Member
            {
                MemberId = GenerateMemberId(DateTime.UtcNow.AddDays(-20)),
                FirstName = "Mohammed",
                LastName = "Khan",
                EmailAddress = "m.khan@email.com",
                PhoneNumber = "+971 52 456 7890",
                DateOfBirth = new DateTime(1998, 12, 10),
                Gender = "Male",
                Nationality = "Pakistani",
                PassportNumber = "PAK456789123",
                EmiratesId = "784-1998-4567891-3",
                Address = "Sharjah University City, Building 5",
                City = "Sharjah",
                Emirate = "Sharjah",
                EmploymentStatus = "Student",
                CompanyName = null,
                VisaType = "Student",
                DoYouKnowAboutTAE = true,
                OptInNidaService = false,
                NidaServiceStatus = NidaServiceStatus.None,
                Advice = "Interested in entrepreneurship and startup opportunities.",
                CreatedAt = DateTime.UtcNow.AddDays(-20)
            },
            new Member
            {
                MemberId = GenerateMemberId(DateTime.UtcNow.AddDays(-15)),
                FirstName = "Fatima",
                LastName = "Al-Zahra",
                EmailAddress = "fatima.alzahra@email.com",
                PhoneNumber = "+971 56 234 5678",
                DateOfBirth = new DateTime(1988, 5, 8),
                Gender = "Female",
                Nationality = "Lebanese",
                PassportNumber = "LEB234567890",
                EmiratesId = "784-1988-2345678-4",
                Address = "Abu Dhabi Marina, Tower B, Floor 25",
                City = "Abu Dhabi",
                Emirate = "Abu Dhabi",
                EmploymentStatus = "Employed",
                CompanyName = "ADNOC Group",
                VisaType = "Employee",
                DoYouKnowAboutTAE = true,
                OptInNidaService = true,
                NidaServiceStatus = NidaServiceStatus.AppointmentScheduled,
                Advice = "Looking for professional development and leadership training.",
                CreatedAt = DateTime.UtcNow.AddDays(-15)
            },
            new Member
            {
                MemberId = GenerateMemberId(DateTime.UtcNow.AddDays(-10)),
                FirstName = "David",
                LastName = "Smith",
                EmailAddress = "david.smith@email.com",
                PhoneNumber = "+971 58 345 6789",
                DateOfBirth = new DateTime(1975, 11, 28),
                Gender = "Male",
                Nationality = "British",
                PassportNumber = "GBR345678901",
                EmiratesId = "784-1975-3456789-5",
                Address = "Dubai Hills Estate, Villa 456",
                City = "Dubai",
                Emirate = "Dubai",
                EmploymentStatus = "Employed",
                CompanyName = "HSBC Middle East",
                VisaType = "Manager",
                DoYouKnowAboutTAE = false,
                OptInNidaService = true,
                NidaServiceStatus = NidaServiceStatus.PendingPayment,
                Advice = "Seeking financial sector networking opportunities.",
                CreatedAt = DateTime.UtcNow.AddDays(-10)
            },
            new Member
            {
                MemberId = GenerateMemberId(DateTime.UtcNow.AddDays(-5)),
                FirstName = "Aisha",
                LastName = "Ibrahim",
                EmailAddress = "aisha.ibrahim@email.com",
                PhoneNumber = "+971 54 456 7890",
                DateOfBirth = new DateTime(1990, 2, 14),
                Gender = "Female",
                Nationality = "Sudanese",
                PassportNumber = "SDN456789012",
                EmiratesId = "784-1990-4567890-6",
                Address = "Ajman Corniche, Building 10",
                City = "Ajman",
                Emirate = "Ajman",
                EmploymentStatus = "Self-employed",
                CompanyName = "Ibrahim Trading Co.",
                VisaType = "Investor",
                DoYouKnowAboutTAE = true,
                OptInNidaService = false,
                NidaServiceStatus = NidaServiceStatus.None,
                Advice = "Interested in women entrepreneurs network and trade opportunities.",
                CreatedAt = DateTime.UtcNow.AddDays(-5)
            }
        };

        context.Members.AddRange(members);
    }

    public static string GenerateMemberId(DateTime createdAt)
    {
        var year = createdAt.Year;
        var random = new Random();
        var number = random.Next(1000, 9999);
        return $"TAE-{year}-{number}";
    }
}
