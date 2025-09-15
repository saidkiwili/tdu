using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using tae_app.Models;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System;

namespace tae_app.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // Normalize DateTime kinds to UTC before saving to the database to satisfy Npgsql timestamptz requirements
    public override int SaveChanges()
    {
        NormalizeDateTimeKinds();
        return base.SaveChanges();
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        NormalizeDateTimeKinds();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        NormalizeDateTimeKinds();
        return base.SaveChangesAsync(cancellationToken);
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        NormalizeDateTimeKinds();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    private void NormalizeDateTimeKinds()
    {
        // Only process entities that are added or modified
        var entries = ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

        foreach (var entry in entries)
        {
            foreach (var prop in entry.Properties)
            {
                if (prop.CurrentValue == null) continue;

                var type = prop.Metadata.ClrType;

                // Handle DateTime
                if (type == typeof(DateTime))
                {
                    var dt = (DateTime)prop.CurrentValue;
                    if (dt.Kind == DateTimeKind.Local)
                    {
                        prop.CurrentValue = dt.ToUniversalTime();
                    }
                    else if (dt.Kind == DateTimeKind.Unspecified)
                    {
                        prop.CurrentValue = DateTime.SpecifyKind(dt, DateTimeKind.Utc);
                    }
                }

                // Handle nullable DateTime
                if (type == typeof(DateTime?))
                {
                    var nullable = (DateTime?)prop.CurrentValue;
                    if (nullable.HasValue)
                    {
                        var dt = nullable.Value;
                        if (dt.Kind == DateTimeKind.Local)
                        {
                            prop.CurrentValue = dt.ToUniversalTime();
                        }
                        else if (dt.Kind == DateTimeKind.Unspecified)
                        {
                            prop.CurrentValue = DateTime.SpecifyKind(dt, DateTimeKind.Utc);
                        }
                    }
                }
            }
        }
    }

    public DbSet<Member> Members { get; set; }
    public DbSet<Appointment> Appointments { get; set; }
    public DbSet<Job> Jobs { get; set; }
    public DbSet<JobCategory> JobCategories { get; set; }
    public DbSet<JobApplication> JobApplications { get; set; }
    public DbSet<AttachmentType> AttachmentTypes { get; set; }
    public DbSet<ApplicationAttachment> ApplicationAttachments { get; set; }
    public DbSet<Event> Events { get; set; }
    public DbSet<EventFormField> EventFormFields { get; set; }
    public DbSet<EventRegistration> EventRegistrations { get; set; }
    public DbSet<EmailSetting> EmailSettings { get; set; }
    public DbSet<OtpVerification> OtpVerifications { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Member configurations
        builder.Entity<Member>(entity =>
        {
            entity.HasIndex(e => e.MemberId).IsUnique();
            entity.HasIndex(e => e.EmailAddress).IsUnique();
            entity.HasIndex(e => e.PhoneNumber).IsUnique();
            entity.HasIndex(e => e.EmiratesId).IsUnique();
            
            entity.Property(e => e.AmountPaid).HasPrecision(18, 2);

            // Configure one-to-one relationship with ApplicationUser
            entity.HasOne(m => m.ApplicationUser)
                .WithOne(u => u.Member)
                .HasForeignKey<Member>(m => m.ApplicationUserId)
                .OnDelete(DeleteBehavior.Cascade); // Delete member when user is deleted
        });

        // Appointment configurations
        builder.Entity<Appointment>(entity =>
        {
            entity.HasOne(a => a.Member)
                .WithMany(m => m.Appointments)
                .HasForeignKey(a => a.MemberId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Job configurations
        builder.Entity<Job>(entity =>
        {
            entity.HasOne(j => j.JobCategory)
                .WithMany(jc => jc.Jobs)
                .HasForeignKey(j => j.JobCategoryId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        builder.Entity<JobCategory>(entity =>
        {
            entity.HasIndex(e => e.Name).IsUnique();
        });

        builder.Entity<JobApplication>(entity =>
        {
            entity.HasOne(ja => ja.Job)
                .WithMany(j => j.JobApplications)
                .HasForeignKey(ja => ja.JobId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(ja => ja.Member)
                .WithMany(m => m.JobApplications)
                .HasForeignKey(ja => ja.MemberId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        builder.Entity<AttachmentType>(entity =>
        {
            entity.HasIndex(e => e.Name).IsUnique();
        });

        builder.Entity<ApplicationAttachment>(entity =>
        {
            entity.HasOne(aa => aa.JobApplication)
                .WithMany(ja => ja.Attachments)
                .HasForeignKey(aa => aa.JobApplicationId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(aa => aa.AttachmentType)
                .WithMany(at => at.Attachments)
                .HasForeignKey(aa => aa.AttachmentTypeId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Event configurations
        builder.Entity<EventFormField>(entity =>
        {
            entity.HasOne(eff => eff.Event)
                .WithMany(e => e.FormFields)
                .HasForeignKey(eff => eff.EventId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<EventRegistration>(entity =>
        {
            entity.HasOne(er => er.Event)
                .WithMany(e => e.Registrations)
                .HasForeignKey(er => er.EventId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(er => er.Member)
                .WithMany(m => m.EventRegistrations)
                .HasForeignKey(er => er.MemberId)
                .OnDelete(DeleteBehavior.Cascade);

            // Ensure one registration per member per event
            entity.HasIndex(e => new { e.EventId, e.MemberId }).IsUnique();
        });

        // OTP Verification configurations
        builder.Entity<OtpVerification>(entity =>
        {
            entity.HasIndex(e => e.Code).IsUnique();
            entity.HasIndex(e => new { e.Email, e.IsUsed });
            entity.HasIndex(e => e.ExpiresAt);

            entity.Property(e => e.Code).HasMaxLength(4);
            entity.Property(e => e.Email).HasMaxLength(256);
        });

        // Email Settings configurations
        builder.Entity<EmailSetting>(entity =>
        {
            entity.HasIndex(e => e.IsActive)
                .HasFilter("[IsActive] = 1");

            entity.Property(e => e.SmtpServer).HasMaxLength(255);
            entity.Property(e => e.Username).HasMaxLength(255);
            entity.Property(e => e.Password).HasMaxLength(255);
            entity.Property(e => e.FromAddress).HasMaxLength(255);
            entity.Property(e => e.FromName).HasMaxLength(255);
        });
    }
}
