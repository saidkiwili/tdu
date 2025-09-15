using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using tae_app.Data;
using tae_app.Models;
using Microsoft.AspNetCore.Authorization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? "Host=localhost;Database=tae_app;Username=postgres;Password=password";

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options => 
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 6;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

builder.Services.AddRazorPages();
builder.Services.AddControllersWithViews();

// Authorization: permission-based handler
// Register the handler as Scoped so it can consume scoped services like UserManager
builder.Services.AddScoped<IAuthorizationHandler, tae_app.Services.RoleClaimsAuthorizationHandler>();
builder.Services.AddAuthorization(options =>
{
    var permissions = new[] {
        "users.view","users.create","users.edit","users.delete",
        "roles.view","roles.create","roles.edit","roles.delete",
        "permissions.view","permissions.create","permissions.edit","permissions.delete",
        "members.view","members.create","members.edit","members.delete",
        "jobs.view","jobs.create","jobs.edit","jobs.delete",
        "events.view","events.create","events.edit","events.delete",
        "settings.view","settings.edit",
        "reports.view","reports.export"
    };

    foreach (var p in permissions)
    {
        options.AddPolicy($"permission:{p}", policy => policy.Requirements.Add(new tae_app.Services.PermissionRequirement(p)));
    }
});

// Register application services
builder.Services.AddScoped<tae_app.Services.IOtpService, tae_app.Services.OtpService>();
builder.Services.AddScoped<tae_app.Services.IEmailService, tae_app.Services.EmailService>();
builder.Services.AddScoped<tae_app.Services.AdminSettingsService>();

var app = builder.Build();

// Apply migrations and seed database
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        
        // Apply pending migrations
        await context.Database.MigrateAsync();
        
        // Seed initial data
        await SeedData.Initialize(services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred migrating or seeding the database.");
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
