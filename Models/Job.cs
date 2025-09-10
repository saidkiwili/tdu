using System.ComponentModel.DataAnnotations;

namespace tae_app.Models;

public class Job
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;
    
    [StringLength(200)]
    public string? Company { get; set; }
    
    [StringLength(100)]
    public string? Location { get; set; }
    
    [Required]
    public string Description { get; set; } = string.Empty;
    
    public DateTime PostedAt { get; set; } = DateTime.UtcNow;
    public DateTime? Deadline { get; set; }
    
    [StringLength(100)]
    public string? SalaryRange { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    // Navigation properties
    public ICollection<JobApplication> JobApplications { get; set; } = new List<JobApplication>();
}

public class JobApplication
{
    public int Id { get; set; }
    
    [Required]
    public int JobId { get; set; }
    [Required]
    public Job Job { get; set; } = null!;
    
    public int? MemberId { get; set; }
    public Member? Member { get; set; }
    
    // For non-members applying
    [StringLength(100)]
    public string? ApplicantName { get; set; }
    
    [EmailAddress]
    [StringLength(255)]
    public string? ApplicantEmail { get; set; }
    
    [StringLength(20)]
    public string? ApplicantPhone { get; set; }
    
    [StringLength(500)]
    public string? CVFilePath { get; set; }
    
    public string? CoverLetter { get; set; }
    
    public DateTime AppliedAt { get; set; } = DateTime.UtcNow;
}
