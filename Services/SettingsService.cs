using tae_app.Data;
using tae_app.Models;
using Microsoft.EntityFrameworkCore;

namespace tae_app.Services;

public class AdminSettingsService
{
    private readonly ApplicationDbContext _context;

    public AdminSettingsService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AdminSettings> GetSettingsAsync()
    {
        var settings = await _context.AdminSettings.FirstOrDefaultAsync();

        if (settings == null)
        {
            // Create default settings if none exist
            settings = new AdminSettings();
            _context.AdminSettings.Add(settings);
            await _context.SaveChangesAsync();
        }

        return settings;
    }

    public async Task<bool> IsNidaServicesEnabledAsync()
    {
        var settings = await GetSettingsAsync();
        return settings.NidaServicesEnabled;
    }

    public async Task<decimal> GetNidaIndividualFeeAsync()
    {
        var settings = await GetSettingsAsync();
        return settings.NidaIndividualFee;
    }

    public async Task<decimal> GetNidaFamilyFeeAsync()
    {
        var settings = await GetSettingsAsync();
        return settings.NidaFamilyFee;
    }

    public async Task<int> GetNidaProcessingTimeAsync()
    {
        var settings = await GetSettingsAsync();
        return settings.NidaProcessingTime;
    }

    public async Task<int> GetNidaMaxApplicationsAsync()
    {
        var settings = await GetSettingsAsync();
        return settings.NidaMaxApplications;
    }

    public async Task<bool> IsJobPortalEnabledAsync()
    {
        var settings = await GetSettingsAsync();
        return settings.JobPortalEnabled;
    }

    public async Task<bool> IsEventsEnabledAsync()
    {
        var settings = await GetSettingsAsync();
        return settings.EventsEnabled;
    }

    public async Task<bool> IsMaintenanceModeAsync()
    {
        var settings = await GetSettingsAsync();
        return settings.MaintenanceMode;
    }

    public async Task UpdateSettingsAsync(AdminSettings updatedSettings, string updatedBy)
    {
        var settings = await GetSettingsAsync();

        // Update all properties
        settings.SiteName = updatedSettings.SiteName;
        settings.SiteDescription = updatedSettings.SiteDescription;
        settings.ContactEmail = updatedSettings.ContactEmail;
        settings.ContactPhone = updatedSettings.ContactPhone;
        settings.MaintenanceMode = updatedSettings.MaintenanceMode;
        settings.NidaServicesEnabled = updatedSettings.NidaServicesEnabled;
        settings.NidaIndividualFee = updatedSettings.NidaIndividualFee;
        settings.NidaFamilyFee = updatedSettings.NidaFamilyFee;
        settings.NidaProcessingTime = updatedSettings.NidaProcessingTime;
        settings.NidaMaxApplications = updatedSettings.NidaMaxApplications;
        settings.JobPortalEnabled = updatedSettings.JobPortalEnabled;
        settings.EventsEnabled = updatedSettings.EventsEnabled;
        settings.RequireStrongPassword = updatedSettings.RequireStrongPassword;
        settings.PasswordExpiration = updatedSettings.PasswordExpiration;
        settings.SessionTimeout = updatedSettings.SessionTimeout;
        settings.MaxLoginAttempts = updatedSettings.MaxLoginAttempts;
        settings.NotifyNewRegistration = updatedSettings.NotifyNewRegistration;
        settings.NotifyNidaApplication = updatedSettings.NotifyNidaApplication;
        settings.AlertSystemErrors = updatedSettings.AlertSystemErrors;
        settings.AlertSecurityThreats = updatedSettings.AlertSecurityThreats;
        settings.LastUpdated = DateTime.UtcNow;
        settings.UpdatedBy = updatedBy;

        await _context.SaveChangesAsync();
    }
}