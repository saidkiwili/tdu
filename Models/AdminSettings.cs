using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace tae_app.Models;

public class AdminSettings
{
    [Key]
    public int Id { get; set; }

    // General Settings
    [Display(Name = "Site Name")]
    [StringLength(100)]
    public string SiteName { get; set; } = "TDUAE - Tanzanian Diaspora UAE";

    [Display(Name = "Site Description")]
    [StringLength(500)]
    public string SiteDescription { get; set; } = "Connecting Tanzanians in the UAE";

    [Display(Name = "Contact Email")]
    [EmailAddress]
    [StringLength(256)]
    public string ContactEmail { get; set; } = "info@tduae.ae";

    [Display(Name = "Contact Phone")]
    [StringLength(20)]
    public string ContactPhone { get; set; } = "+971-XX-XXXXXXX";

    [Display(Name = "Maintenance Mode")]
    public bool MaintenanceMode { get; set; } = false;

    // Service Settings
    [Display(Name = "NIDA Services Enabled")]
    public bool NidaServicesEnabled { get; set; } = true;

    [Display(Name = "NIDA Individual Fee")]
    [Range(0, 10000)]
    [Column(TypeName = "decimal(18,2)")]
    public decimal NidaIndividualFee { get; set; } = 150.00M;

    [Display(Name = "NIDA Family Fee")]
    [Range(0, 10000)]
    [Column(TypeName = "decimal(18,2)")]
    public decimal NidaFamilyFee { get; set; } = 400.00M;

    [Display(Name = "NIDA Processing Time (Days)")]
    [Range(1, 365)]
    public int NidaProcessingTime { get; set; } = 14;

    [Display(Name = "NIDA Max Applications/Day")]
    [Range(1, 1000)]
    public int NidaMaxApplications { get; set; } = 50;

    [Display(Name = "Job Portal Enabled")]
    public bool JobPortalEnabled { get; set; } = true;

    [Display(Name = "Events Enabled")]
    public bool EventsEnabled { get; set; } = true;

    // Security Settings
    [Display(Name = "Require Strong Password")]
    public bool RequireStrongPassword { get; set; } = true;

    [Display(Name = "Password Expiration")]
    public bool PasswordExpiration { get; set; } = true;

    [Display(Name = "Session Timeout (minutes)")]
    [Range(5, 480)]
    public int SessionTimeout { get; set; } = 30;

    [Display(Name = "Max Login Attempts")]
    [Range(3, 20)]
    public int MaxLoginAttempts { get; set; } = 5;

    // Notification Settings
    [Display(Name = "Notify New Registration")]
    public bool NotifyNewRegistration { get; set; } = true;

    [Display(Name = "Notify NIDA Application")]
    public bool NotifyNidaApplication { get; set; } = true;

    [Display(Name = "Alert System Errors")]
    public bool AlertSystemErrors { get; set; } = true;

    [Display(Name = "Alert Security Threats")]
    public bool AlertSecurityThreats { get; set; } = true;

    [Display(Name = "Last Updated")]
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

    [Display(Name = "Updated By")]
    [StringLength(256)]
    public string? UpdatedBy { get; set; }
}
