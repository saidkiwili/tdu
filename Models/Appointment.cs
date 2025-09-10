using System.ComponentModel.DataAnnotations;

namespace tae_app.Models;

public enum AppointmentStatus
{
    Scheduled = 1,
    Confirmed = 2,
    InProgress = 3,
    Completed = 4,
    Cancelled = 5,
    NoShow = 6
}

public class Appointment
{
    public int Id { get; set; }
    
    [Required]
    public int MemberId { get; set; }
    [Required]
    public Member Member { get; set; } = null!;
    
    [Required]
    [StringLength(100)]
    public string ServiceType { get; set; } = string.Empty; // e.g., "NIDA Service"
    
    [Required]
    public DateTime ScheduledAt { get; set; }
    
    [StringLength(200)]
    public string? Location { get; set; }
    
    public AppointmentStatus Status { get; set; } = AppointmentStatus.Scheduled;
    
    [StringLength(500)]
    public string? Notes { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
