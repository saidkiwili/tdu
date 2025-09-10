using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace tae_app.Models;

public enum NidaServiceStatus
{
    None = 0,
    PendingPayment = 1,
    Paid = 2,
    AppointmentScheduled = 3,
    Completed = 4
}

public class Member
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(50)]
    public string MemberId { get; set; } = string.Empty; // Auto-generated: TAE-{YYYY}-{NNNN}
    
    // Optional link to Identity user (for members who create accounts)
    public string? ApplicationUserId { get; set; }
    [ForeignKey("ApplicationUserId")]
    public ApplicationUser? ApplicationUser { get; set; }
    
    [Required]
    [StringLength(100)]
    public string FirstName { get; set; } = string.Empty;
    
    [StringLength(100)]
    public string? MiddleName { get; set; }
    
    [Required]
    [StringLength(100)]
    public string LastName { get; set; } = string.Empty;
    
    public DateTime? DateOfBirth { get; set; }
    
    [StringLength(50)]
    public string? Gender { get; set; }
    
    [StringLength(100)]
    public string? Nationality { get; set; }
    
    [StringLength(50)]
    public string? PassportNumber { get; set; }
    
    [StringLength(50)]
    public string? EmiratesId { get; set; }
    
    [StringLength(500)]
    public string? Address { get; set; }
    
    [StringLength(100)]
    public string? City { get; set; }
    
    [StringLength(100)]
    public string? Emirate { get; set; }
    
    [Required]
    [StringLength(20)]
    public string PhoneNumber { get; set; } = string.Empty;
    
    [Required]
    [EmailAddress]
    [StringLength(255)]
    public string EmailAddress { get; set; } = string.Empty;
    
    [StringLength(100)]
    public string? EmploymentStatus { get; set; }
    
    [StringLength(200)]
    public string? CompanyName { get; set; }
    
    public bool DoYouKnowAboutTAE { get; set; }
    
    [StringLength(100)]
    public string? VisaType { get; set; }
    
    [StringLength(500)]
    public string? VisaIdFilePath { get; set; }
    
    [StringLength(1000)]
    public string? Advice { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // NIDA Service fields
    public bool OptInNidaService { get; set; }
    public NidaServiceStatus NidaServiceStatus { get; set; } = NidaServiceStatus.None;
    public string? PaymentReference { get; set; }
    public decimal? AmountPaid { get; set; }
    public string? PaymentMethod { get; set; }
    
    // Navigation properties
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    public ICollection<EventRegistration> EventRegistrations { get; set; } = new List<EventRegistration>();
    public ICollection<JobApplication> JobApplications { get; set; } = new List<JobApplication>();
    
    // NotMapped helper properties
    [NotMapped]
    public string FullName => $"{FirstName} {MiddleName} {LastName}".Trim();
    
    [NotMapped]
    public IFormFile? VisaIdFile { get; set; }
}
