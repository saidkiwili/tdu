using System.ComponentModel.DataAnnotations;

namespace tae_app.Models;

public class Event
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;
    
    public string? Description { get; set; }
    
    [StringLength(200)]
    public string? Venue { get; set; }
    
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    
    public bool IsPublic { get; set; } = true;
    public bool MemberOnly { get; set; } = false;
    
    public int? Capacity { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public ICollection<EventFormField> FormFields { get; set; } = new List<EventFormField>();
    public ICollection<EventRegistration> Registrations { get; set; } = new List<EventRegistration>();
}

public enum FieldType
{
    Text = 1,
    Number = 2,
    Select = 3,
    Checkbox = 4,
    File = 5
}

public class EventFormField
{
    public int Id { get; set; }
    
    [Required]
    public int EventId { get; set; }
    [Required]
    public Event Event { get; set; } = null!;
    
    [Required]
    [StringLength(100)]
    public string Label { get; set; } = string.Empty;
    
    public FieldType FieldType { get; set; }
    
    public bool IsRequired { get; set; }
    
    public string? Options { get; set; } // JSON for select options
    
    public int Order { get; set; }
}

public class EventRegistration
{
    public int Id { get; set; }
    
    [Required]
    public int EventId { get; set; }
    [Required]
    public Event Event { get; set; } = null!;
    
    [Required]
    public int MemberId { get; set; }
    [Required]
    public Member Member { get; set; } = null!;
    
    public string? FormResponses { get; set; } // JSON of responses to dynamic fields
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
