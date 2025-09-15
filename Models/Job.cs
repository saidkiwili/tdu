using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace tae_app.Models;

public class Job
{
    public int Id { get; set; }

    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Company { get; set; } = string.Empty;

    [Required]
    public string Description { get; set; } = string.Empty;

    [Required]
    public string Requirements { get; set; } = string.Empty;

    [StringLength(100)]
    public string? Location { get; set; }

    [StringLength(50)]
    public string? JobType { get; set; } // Full-time, Part-time, Contract, etc.

    [StringLength(50)]
    public string? ExperienceLevel { get; set; } // Entry, Mid, Senior, Executive

    [Column(TypeName = "decimal(18,2)")]
    public decimal? SalaryMin { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? SalaryMax { get; set; }

    [StringLength(10)]
    public string Currency { get; set; } = "AED";

    public string? Benefits { get; set; }

    [EmailAddress]
    public string? ContactEmail { get; set; }

    [StringLength(20)]
    public string? ContactPhone { get; set; }

    public string? ImageUrl { get; set; }

    public bool IsActive { get; set; } = true;

    public bool IsFeatured { get; set; } = false;

    public DateTime PostedAt { get; set; } = DateTime.UtcNow;

    public DateTime? Deadline { get; set; }

    // Foreign Keys
    public int? JobCategoryId { get; set; }

    [ForeignKey("JobCategoryId")]
    public virtual JobCategory? JobCategory { get; set; }

    // Navigation properties
    public virtual ICollection<JobApplication> JobApplications { get; set; } = new List<JobApplication>();
}

public class JobCategory
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    public string? IconClass { get; set; } // FontAwesome icon class

    public bool IsActive { get; set; } = true;

    // Navigation properties
    public virtual ICollection<Job> Jobs { get; set; } = new List<Job>();
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

    public string? CoverLetter { get; set; }

    public string? Status { get; set; } = "Pending"; // Pending, Reviewed, Accepted, Rejected

    public DateTime AppliedAt { get; set; } = DateTime.UtcNow;

    public DateTime? ReviewedAt { get; set; }

    public string? ReviewNotes { get; set; }

    // Navigation properties
    public virtual ICollection<ApplicationAttachment> Attachments { get; set; } = new List<ApplicationAttachment>();
}

public class AttachmentType
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    [Required]
    [StringLength(255)]
    public string AllowedExtensions { get; set; } = string.Empty; // e.g., ".pdf,.doc,.docx"

    public long MaxFileSize { get; set; } = 5242880; // 5MB default

    public bool IsRequired { get; set; } = false;

    public bool IsActive { get; set; } = true;

    // Navigation properties
    public virtual ICollection<ApplicationAttachment> Attachments { get; set; } = new List<ApplicationAttachment>();
}

public class ApplicationAttachment
{
    public int Id { get; set; }

    [Required]
    public int JobApplicationId { get; set; }

    [ForeignKey("JobApplicationId")]
    public virtual JobApplication JobApplication { get; set; } = null!;

    [Required]
    public int AttachmentTypeId { get; set; }

    [ForeignKey("AttachmentTypeId")]
    public virtual AttachmentType AttachmentType { get; set; } = null!;

    [Required]
    [StringLength(255)]
    public string FileName { get; set; } = string.Empty;

    [Required]
    [StringLength(500)]
    public string FilePath { get; set; } = string.Empty;

    public long FileSize { get; set; }

    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
}
