using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using tae_app.Data;
using tae_app.Models;

namespace tae_app.Services;

public interface IOtpService
{
    Task<string> GenerateOtpAsync(string email);
    Task<bool> VerifyOtpAsync(string email, string code);
    Task<bool> IsOtpValidAsync(string email, string code);
    Task ExpireOldOtpsAsync();
    Task SendOtpEmailAsync(string email, string otp);
    Task<bool> ResendOtpAsync(string email);
}

public class OtpService : IOtpService
{
    private readonly ApplicationDbContext _context;
    private readonly IEmailService _emailService;
    private readonly ILogger<OtpService> _logger;

    public OtpService(
        ApplicationDbContext context,
        IEmailService emailService,
        ILogger<OtpService> logger)
    {
        _context = context;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<string> GenerateOtpAsync(string email)
    {
        // Expire any existing OTPs for this email
        await ExpireExistingOtpsAsync(email);

        // Generate a unique 4-digit OTP
        string otp;
        bool isUnique;
        int attempts = 0;
        const int maxAttempts = 10;

        do
        {
            otp = GenerateRandomOtp();
            isUnique = !await _context.OtpVerifications.AnyAsync(o => o.Code == otp);
            attempts++;
        } while (!isUnique && attempts < maxAttempts);

        if (!isUnique)
        {
            throw new Exception("Unable to generate unique OTP after maximum attempts");
        }

        // Create OTP record
        var otpVerification = new OtpVerification
        {
            Code = otp,
            Email = email,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddMinutes(5), // 5 minutes expiry
            IsUsed = false
        };

        _context.OtpVerifications.Add(otpVerification);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Generated OTP for email: {Email}", email);

        return otp;
    }

    public async Task<bool> VerifyOtpAsync(string email, string code)
    {
        var otpVerification = await _context.OtpVerifications
            .FirstOrDefaultAsync(o =>
                o.Email == email &&
                o.Code == code &&
                !o.IsUsed &&
                o.ExpiresAt > DateTime.UtcNow);

        if (otpVerification == null)
        {
            return false;
        }

        // Mark OTP as used
        otpVerification.IsUsed = true;
        otpVerification.UsedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("OTP verified successfully for email: {Email}", email);

        return true;
    }

    public async Task<bool> IsOtpValidAsync(string email, string code)
    {
        return await _context.OtpVerifications
            .AnyAsync(o =>
                o.Email == email &&
                o.Code == code &&
                !o.IsUsed &&
                o.ExpiresAt > DateTime.UtcNow);
    }

    public async Task ExpireOldOtpsAsync()
    {
        var expiredOtps = await _context.OtpVerifications
            .Where(o => o.ExpiresAt <= DateTime.UtcNow && !o.IsUsed)
            .ToListAsync();

        if (expiredOtps.Any())
        {
            foreach (var otp in expiredOtps)
            {
                otp.IsUsed = true; // Mark as used to prevent reuse
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Expired {Count} old OTPs", expiredOtps.Count);
        }
    }

    public async Task SendOtpEmailAsync(string email, string otp)
    {
        var subject = "TDUAE Registration - Email Verification Code";
        var body = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Email Verification</title>
</head>
<body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; padding: 20px;'>
    <div style='background: linear-gradient(135deg, #1e40af 0%, #3b82f6 100%); padding: 30px; text-align: center; border-radius: 10px 10px 0 0;'>
        <h1 style='color: white; margin: 0; font-size: 24px;'>Tanzania Diaspora UAE</h1>
        <p style='color: #e0f2fe; margin: 10px 0 0 0; font-size: 16px;'>Email Verification</p>
    </div>

    <div style='background: white; border: 1px solid #e5e7eb; border-radius: 0 0 10px 10px; padding: 30px;'>
        <h2 style='color: #1f2937; margin-top: 0;'>Verify Your Email Address</h2>

        <p style='font-size: 16px; margin-bottom: 20px;'>
            Thank you for registering with the Tanzania Expatriates Association. To complete your registration, please use the verification code below:
        </p>

        <div style='background: #f8fafc; border: 2px solid #e2e8f0; border-radius: 8px; padding: 20px; text-align: center; margin: 20px 0;'>
            <h1 style='color: #1e40af; font-size: 32px; margin: 0; letter-spacing: 5px; font-weight: bold;'>{otp}</h1>
        </div>

        <div style='background: #fef3c7; border: 1px solid #f59e0b; border-radius: 6px; padding: 15px; margin: 20px 0;'>
            <p style='margin: 0; color: #92400e; font-weight: 500;'>
                <strong>⚠️ Important:</strong> This code will expire in <strong>5 minutes</strong> for security reasons.
            </p>
        </div>

        <p style='font-size: 14px; color: #6b7280; margin-top: 20px;'>
            If you didn't request this verification code, please ignore this email.
        </p>

        <hr style='border: none; border-top: 1px solid #e5e7eb; margin: 30px 0;'>

        <div style='text-align: center; color: #6b7280; font-size: 12px;'>
            <p style='margin: 5px 0;'>© 2025 Tanzania Diaspora UAE</p>
            <p style='margin: 5px 0;'>This is an automated message. Please do not reply.</p>
        </div>
    </div>
</body>
</html>";

        await _emailService.SendEmailAsync(email, subject, body);
        _logger.LogInformation("OTP email sent to: {Email}", email);
    }

    public async Task<bool> ResendOtpAsync(string email)
    {
        // Check if there's a recent OTP that can be resent
        var recentOtp = await _context.OtpVerifications
            .Where(o => o.Email == email && !o.IsUsed)
            .OrderByDescending(o => o.CreatedAt)
            .FirstOrDefaultAsync();

        if (recentOtp == null)
        {
            return false; // No OTP to resend
        }

        // Check if the OTP was created within the last minute to prevent spam
        if (recentOtp.CreatedAt > DateTime.UtcNow.AddMinutes(-1))
        {
            throw new Exception("Please wait at least 1 minute before requesting a new code.");
        }

        // Resend the existing OTP
        await SendOtpEmailAsync(email, recentOtp.Code);

        _logger.LogInformation("OTP resent to email: {Email}", email);

        return true;
    }

    private async Task ExpireExistingOtpsAsync(string email)
    {
        var existingOtps = await _context.OtpVerifications
            .Where(o => o.Email == email && !o.IsUsed)
            .ToListAsync();

        foreach (var otp in existingOtps)
        {
            otp.IsUsed = true; // Mark as used to prevent reuse
        }

        if (existingOtps.Any())
        {
            await _context.SaveChangesAsync();
            _logger.LogInformation("Expired {Count} existing OTPs for email: {Email}", existingOtps.Count, email);
        }
    }

    private string GenerateRandomOtp()
    {
        // Generate a random 4-digit number
        using (var rng = RandomNumberGenerator.Create())
        {
            var bytes = new byte[2];
            rng.GetBytes(bytes);
            var number = BitConverter.ToUInt16(bytes, 0) % 10000;
            return number.ToString("D4");
        }
    }
}
