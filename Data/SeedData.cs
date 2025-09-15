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

        // Seed job portal data
        await SeedJobCategories(context);
        await SeedAttachmentTypes(context);
        await SeedSampleJobs(context);

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
                await userManager.AddToRoleAsync(adminUser, "Admin");
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

    private static async Task SeedJobCategories(ApplicationDbContext context)
    {
        // Check if job categories already exist
        if (await context.JobCategories.AnyAsync())
        {
            return;
        }

        var categories = new[]
        {
            new JobCategory
            {
                Name = "Technology",
                Description = "IT, Software Development, Data Science, Cybersecurity",
                IconClass = "fas fa-code",
                IsActive = true
            },
            new JobCategory
            {
                Name = "Finance",
                Description = "Banking, Accounting, Financial Services, Investment",
                IconClass = "fas fa-dollar-sign",
                IsActive = true
            },
            new JobCategory
            {
                Name = "Healthcare",
                Description = "Medical, Nursing, Healthcare Services, Pharmaceuticals",
                IconClass = "fas fa-stethoscope",
                IsActive = true
            },
            new JobCategory
            {
                Name = "Education",
                Description = "Teaching, Training, Education Services, Administration",
                IconClass = "fas fa-graduation-cap",
                IsActive = true
            },
            new JobCategory
            {
                Name = "Engineering",
                Description = "Civil, Mechanical, Electrical, Petroleum Engineering",
                IconClass = "fas fa-cogs",
                IsActive = true
            },
            new JobCategory
            {
                Name = "Marketing",
                Description = "Digital Marketing, Brand Management, Advertising",
                IconClass = "fas fa-bullhorn",
                IsActive = true
            },
            new JobCategory
            {
                Name = "Construction",
                Description = "Project Management, Architecture, Real Estate",
                IconClass = "fas fa-building",
                IsActive = true
            },
            new JobCategory
            {
                Name = "Hospitality",
                Description = "Hotels, Restaurants, Tourism, Event Management",
                IconClass = "fas fa-concierge-bell",
                IsActive = true
            }
        };

        context.JobCategories.AddRange(categories);
    }

    private static async Task SeedAttachmentTypes(ApplicationDbContext context)
    {
        // Check if attachment types already exist
        if (await context.AttachmentTypes.AnyAsync())
        {
            return;
        }

        var attachmentTypes = new[]
        {
            new AttachmentType
            {
                Name = "CV/Resume",
                Description = "Curriculum Vitae or Resume document",
                AllowedExtensions = ".pdf,.doc,.docx",
                MaxFileSize = 5242880, // 5MB
                IsRequired = true,
                IsActive = true
            },
            new AttachmentType
            {
                Name = "Cover Letter",
                Description = "Cover letter or motivation letter",
                AllowedExtensions = ".pdf,.doc,.docx,.txt",
                MaxFileSize = 2097152, // 2MB
                IsRequired = false,
                IsActive = true
            },
            new AttachmentType
            {
                Name = "Portfolio",
                Description = "Portfolio, samples of work, or project examples",
                AllowedExtensions = ".pdf,.zip,.rar,.jpg,.jpeg,.png",
                MaxFileSize = 10485760, // 10MB
                IsRequired = false,
                IsActive = true
            },
            new AttachmentType
            {
                Name = "Certificates",
                Description = "Professional certificates, licenses, or qualifications",
                AllowedExtensions = ".pdf,.jpg,.jpeg,.png",
                MaxFileSize = 5242880, // 5MB
                IsRequired = false,
                IsActive = true
            },
            new AttachmentType
            {
                Name = "References",
                Description = "Professional references or recommendation letters",
                AllowedExtensions = ".pdf,.doc,.docx",
                MaxFileSize = 3145728, // 3MB
                IsRequired = false,
                IsActive = true
            }
        };

        context.AttachmentTypes.AddRange(attachmentTypes);
    }

    private static async Task SeedSampleJobs(ApplicationDbContext context)
    {
        // Check if jobs already exist
        if (await context.Jobs.AnyAsync())
        {
            return;
        }

        var technologyCategory = await context.JobCategories.FirstOrDefaultAsync(c => c.Name == "Technology");
        var financeCategory = await context.JobCategories.FirstOrDefaultAsync(c => c.Name == "Finance");
        var healthcareCategory = await context.JobCategories.FirstOrDefaultAsync(c => c.Name == "Healthcare");
        var educationCategory = await context.JobCategories.FirstOrDefaultAsync(c => c.Name == "Education");

        if (technologyCategory == null || financeCategory == null || healthcareCategory == null || educationCategory == null)
        {
            return; // Categories not seeded yet
        }

        var jobs = new[]
        {
            new Job
            {
                Title = "Senior Software Developer",
                Company = "Tech Solutions Inc.",
                Description = @"We are looking for an experienced Senior Software Developer to join our dynamic team.

Key Responsibilities:
• Design and develop high-quality software solutions
• Collaborate with cross-functional teams
• Participate in code reviews and mentoring junior developers
• Implement best practices and coding standards
• Contribute to architectural decisions

Requirements:
• 5+ years of experience in software development
• Strong proficiency in C#, .NET Core, and ASP.NET
• Experience with modern JavaScript frameworks (React, Angular, or Vue.js)
• Knowledge of cloud platforms (Azure, AWS)
• Excellent problem-solving and communication skills

Benefits:
• Competitive salary and performance bonuses
• Health insurance and retirement plan
• Flexible working hours and remote work options
• Professional development opportunities
• Modern office with great amenities",
                Requirements = @"• Bachelor's degree in Computer Science or related field
• 5+ years of experience in software development
• Strong proficiency in C#, .NET Core, and ASP.NET
• Experience with modern JavaScript frameworks
• Knowledge of cloud platforms (Azure, AWS preferred)
• Experience with SQL Server and NoSQL databases
• Familiarity with Agile/Scrum methodologies
• Excellent problem-solving and communication skills",
                Location = "Dubai, UAE",
                JobType = "Full-time",
                ExperienceLevel = "Senior",
                SalaryMin = 25000,
                SalaryMax = 40000,
                Currency = "AED",
                Benefits = @"• Competitive salary package
• Annual performance bonus
• Health insurance coverage
• Professional development budget
• Flexible working arrangements
• Modern office facilities",
                ContactEmail = "careers@techsolutions.ae",
                ContactPhone = "+971 4 123 4567",
                IsActive = true,
                IsFeatured = true,
                PostedAt = DateTime.UtcNow.AddDays(-2),
                Deadline = DateTime.UtcNow.AddDays(28),
                JobCategoryId = technologyCategory.Id
            },
            new Job
            {
                Title = "Financial Analyst",
                Company = "Global Finance Corp",
                Description = @"Join our finance team as a Financial Analyst and contribute to strategic financial planning and analysis.

Key Responsibilities:
• Analyze financial data and prepare reports
• Support budgeting and forecasting processes
• Conduct financial modeling and scenario analysis
• Assist in investment analysis and recommendations
• Prepare presentations for senior management

Requirements:
• Bachelor's degree in Finance, Accounting, or related field
• 3+ years of experience in financial analysis
• Strong Excel skills and financial modeling experience
• Knowledge of financial software and ERP systems
• Excellent analytical and communication skills

Benefits:
• Competitive salary and bonus structure
• Professional certification support
• Health and life insurance
• Flexible working hours
• Career advancement opportunities",
                Requirements = @"• Bachelor's degree in Finance, Accounting, or related field
• 3+ years of experience in financial analysis
• Advanced Excel skills and financial modeling
• Knowledge of Bloomberg, Reuters, or similar platforms
• Experience with ERP systems (SAP, Oracle preferred)
• CFA or CPA certification preferred
• Strong analytical and communication skills",
                Location = "Abu Dhabi, UAE",
                JobType = "Full-time",
                ExperienceLevel = "Mid",
                SalaryMin = 20000,
                SalaryMax = 30000,
                Currency = "AED",
                Benefits = @"• Competitive compensation package
• Performance-based bonuses
• Professional development support
• Comprehensive health insurance
• Flexible work arrangements
• Career progression opportunities",
                ContactEmail = "hr@globalfinance.ae",
                ContactPhone = "+971 2 765 4321",
                IsActive = true,
                IsFeatured = false,
                PostedAt = DateTime.UtcNow.AddDays(-5),
                Deadline = DateTime.UtcNow.AddDays(25),
                JobCategoryId = financeCategory.Id
            },
            new Job
            {
                Title = "Registered Nurse",
                Company = "MedCare Hospital",
                Description = @"We are seeking compassionate and skilled Registered Nurses to join our healthcare team.

Key Responsibilities:
• Provide direct patient care and monitoring
• Administer medications and treatments
• Collaborate with healthcare team members
• Maintain accurate patient records
• Educate patients and families about health conditions

Requirements:
• Valid nursing license and DHA eligibility
• 2+ years of clinical experience preferred
• Strong communication and interpersonal skills
• Ability to work in fast-paced environment
• Commitment to patient-centered care

Benefits:
• Competitive salary and shift allowances
• Health insurance and malpractice coverage
• Professional development opportunities
• Accommodation allowance
• Annual leave and sick leave",
                Requirements = @"• Valid nursing license
• DHA/MOH eligibility letter
• BLS/ACLS certification
• 2+ years of clinical experience preferred
• Excellent communication skills
• Ability to work rotating shifts
• Commitment to quality patient care",
                Location = "Dubai, UAE",
                JobType = "Full-time",
                ExperienceLevel = "Mid",
                SalaryMin = 15000,
                SalaryMax = 22000,
                Currency = "AED",
                Benefits = @"• Competitive salary with shift allowances
• Comprehensive health insurance
• Professional liability coverage
• Continuing education support
• Accommodation assistance
• Annual leave entitlement",
                ContactEmail = "nursing@medcare.ae",
                ContactPhone = "+971 4 987 6543",
                IsActive = true,
                IsFeatured = false,
                PostedAt = DateTime.UtcNow.AddDays(-3),
                Deadline = DateTime.UtcNow.AddDays(30),
                JobCategoryId = healthcareCategory.Id
            },
            new Job
            {
                Title = "Mathematics Teacher",
                Company = "International School Dubai",
                Description = @"Join our academic team as a Mathematics Teacher and inspire students to excel in mathematics.

Key Responsibilities:
• Teach mathematics to secondary school students
• Develop engaging lesson plans and teaching materials
• Assess student progress and provide feedback
• Participate in curriculum development
• Support student academic and personal development

Requirements:
• Bachelor's degree in Mathematics or Education
• Teaching certification preferred
• 2+ years of teaching experience
• Strong subject knowledge and teaching skills
• Excellent classroom management abilities

Benefits:
• Competitive salary and annual increments
• Health insurance and retirement plan
• Professional development opportunities
• Annual leave and public holidays
• Modern teaching facilities",
                Requirements = @"• Bachelor's degree in Mathematics or related field
• Teaching qualification (PGCE, B.Ed, or equivalent)
• 2+ years of teaching experience preferred
• Strong mathematical knowledge
• Excellent communication and classroom management skills
• Experience with modern teaching methodologies",
                Location = "Dubai, UAE",
                JobType = "Full-time",
                ExperienceLevel = "Mid",
                SalaryMin = 12000,
                SalaryMax = 18000,
                Currency = "AED",
                Benefits = @"• Competitive salary package
• Annual salary increments
• Comprehensive health insurance
• Professional development funding
• Generous annual leave
• State-of-the-art facilities",
                ContactEmail = "teaching@isd.ae",
                ContactPhone = "+971 4 555 1234",
                IsActive = true,
                IsFeatured = false,
                PostedAt = DateTime.UtcNow.AddDays(-7),
                Deadline = DateTime.UtcNow.AddDays(20),
                JobCategoryId = educationCategory.Id
            }
        };

        context.Jobs.AddRange(jobs);
    }
}
