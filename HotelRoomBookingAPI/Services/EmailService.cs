using System.Net;
using System.Net.Mail;

namespace HotelRoomBookingAPI.Services;

public interface IEmailService
{
    Task SendEmailAsync(string toEmail, string subject, string message);
}

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendEmailAsync(string toEmail, string subject, string messageBody)
    {
        var smtpHost = _configuration["EmailSettings:SmtpHost"];
        var smtpPort = _configuration.GetValue<int>("EmailSettings:SmtpPort");
        var smtpUser = _configuration["EmailSettings:SmtpUser"];
        var smtpPass = _configuration["EmailSettings:SmtpPass"];
        var fromEmail = _configuration["EmailSettings:FromEmail"] ?? "noreply@hotelbooking.com";

        // If SMTP not configured, log to console (Fallback for testing)
        if (string.IsNullOrEmpty(smtpHost))
        {
            _logger.LogWarning("SMTP not configured. Email to {Email} with subject '{Subject}' was NOT sent via network.", toEmail, subject);
            _logger.LogInformation("------------ EMAIL CONTENT ------------");
            _logger.LogInformation("To: {Email}", toEmail);
            _logger.LogInformation("Subject: {Subject}", subject);
            _logger.LogInformation("Body: {Body}", messageBody);
            _logger.LogInformation("---------------------------------------");
            return;
        }

        try
        {
            using var client = new SmtpClient(smtpHost, smtpPort)
            {
                Credentials = new NetworkCredential(smtpUser, smtpPass),
                EnableSsl = _configuration.GetValue<bool>("EmailSettings:EnableSsl", true)
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(fromEmail),
                Subject = subject,
                Body = messageBody,
                IsBodyHtml = true,
            };

            mailMessage.To.Add(toEmail);

            await client.SendMailAsync(mailMessage);
            _logger.LogInformation("Email sent successfully to {Email}", toEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Email}", toEmail);
            // Don't throw, just log. Or throw if critical. For forgot password, maybe we should know if it failed.
            // But usually we don't want to crash the request.
            throw; 
        }
    }
}
