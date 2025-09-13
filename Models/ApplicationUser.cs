using Microsoft.AspNetCore.Identity;

namespace tae_app.Models;

public class ApplicationUser : IdentityUser
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
    // Optional avatar path stored relative to wwwroot, e.g. "uploads/avatars/{userid}.jpg"
    public string? AvatarPath { get; set; }
    
    // Navigation property to Member
    public Member? Member { get; set; }
}
