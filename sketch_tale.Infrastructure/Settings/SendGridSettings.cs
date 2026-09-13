namespace sketch_tale.Infrastructure.Settings;

public class SendGridSettings
{
    public string ApiKey { get; set; } = null!;
    public string FromEmail { get; set; } = null!;
    public string FromName { get; set; } = null!;
}
