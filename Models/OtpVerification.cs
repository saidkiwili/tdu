using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace tae_app.Models;

public class OtpVerification
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(4, MinimumLength = 4)]
    public string Code { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Required]
    public DateTime ExpiresAt { get; set; }

    public bool IsUsed { get; set; } = false;

    public DateTime? UsedAt { get; set; }

    // Store registration data temporarily as JSON
    public string? RegistrationData { get; set; }

    // Navigation property for related member (if registration completes)
    public int? MemberId { get; set; }

    [ForeignKey("MemberId")]
    public virtual Member? Member { get; set; }

    // Check if OTP is expired
    public bool IsExpired => DateTime.UtcNow > ExpiresAt;

    // Check if OTP is valid (not used and not expired)
    public bool IsValid => !IsUsed && !IsExpired;
}
