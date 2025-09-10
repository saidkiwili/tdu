using System.ComponentModel.DataAnnotations;

namespace tae_app.Models
{
    public class EmailSetting
    {
        public int Id { get; set; }

        [Required]
        [StringLength(255)]
        public string SmtpServer { get; set; } = "";

        [Range(1, 65535)]
        public int SmtpPort { get; set; } = 587;

        [Required]
        [StringLength(255)]
        public string Username { get; set; } = "";

        [Required]
        [StringLength(255)]
        public string Password { get; set; } = ""; // Should be encrypted in production

        [Required]
        [StringLength(255)]
        [EmailAddress]
        public string FromAddress { get; set; } = "";

        [StringLength(255)]
        public string FromName { get; set; } = "";

        public bool UseSsl { get; set; } = true;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
