using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;
using sketch_tale.Infrastructure.Settings;
using sketch_tale.Application.Interfaces.Services;

namespace sketch_tale.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly SendGridSettings _settings;

    public EmailService(IOptions<SendGridSettings> settings)
    {
        _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
    }

    public async Task SendEmailAsync(string toEmail, string subject, string body)
    {
        if (string.IsNullOrWhiteSpace(_settings.ApiKey))
        {
            throw new InvalidOperationException(
                "Missing SendGrid ApiKey. Set SendGrid:ApiKey in appsettings or environment variables.");
        }

        if (string.IsNullOrWhiteSpace(_settings.FromEmail))
        {
            throw new InvalidOperationException(
                "Missing SendGrid FromEmail. Set SendGrid:FromEmail in appsettings or environment variables.");
        }

        var client = new SendGridClient(_settings.ApiKey);

        var from = new EmailAddress(_settings.FromEmail, _settings.FromName);
        var to = new EmailAddress(toEmail);

        var msg = MailHelper.CreateSingleEmail(from, to, subject, body, body);

        await client.SendEmailAsync(msg);
    }
}
