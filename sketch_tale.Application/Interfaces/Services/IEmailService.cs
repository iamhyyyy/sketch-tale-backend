
namespace sketch_tale.Application.Interfaces.Services;
public interface IEmailService
{
    Task SendEmailAsync(string toEmail, string subject, string body);
}

