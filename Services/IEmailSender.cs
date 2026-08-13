namespace Taskify.Services;

public interface IEmailSender
{
    bool IsConfigured { get; }

    Task SendAsync(string to, string subject, string htmlBody, CancellationToken cancellationToken = default);
}
