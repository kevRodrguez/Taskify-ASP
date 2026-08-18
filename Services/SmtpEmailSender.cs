using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using Taskify.Configuration;

namespace Taskify.Services;

public sealed class SmtpEmailSender : IEmailSender
{
    private readonly EmailSettings _settings;
    private readonly ILogger<SmtpEmailSender> _logger;

    public SmtpEmailSender(IOptions<EmailSettings> settings, ILogger<SmtpEmailSender> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(_settings.SmtpHost) && !string.IsNullOrWhiteSpace(_settings.From);

    public async Task SendAsync(string to, string subject, string htmlBody, CancellationToken cancellationToken = default)
    {
        if (!IsConfigured)
        {
            _logger.LogInformation("SMTP no configurado; se omite el correo '{Subject}' a {To}.", subject, to);
            return;
        }

        var message = new MimeMessage();
        message.From.Add(MailboxAddress.Parse(_settings.From));
        message.To.Add(MailboxAddress.Parse(to));
        message.Subject = subject;
        message.Body = new BodyBuilder { HtmlBody = htmlBody }.ToMessageBody();

        using var client = new SmtpClient();
        client.Timeout = 20_000;
        // macOS/.NET often fails Gmail's CRL/OCSP check ("incomplete certificate revocation check").
        client.CheckCertificateRevocation = false;
        await client.ConnectAsync(_settings.SmtpHost, _settings.SmtpPort, ResolveSocketOptions(), cancellationToken);

        if (!string.IsNullOrWhiteSpace(_settings.User))
        {
            var password = _settings.Password.Replace(" ", string.Empty);
            await client.AuthenticateAsync(_settings.User, password, cancellationToken);
        }

        await client.SendAsync(message, cancellationToken);
        await client.DisconnectAsync(true, cancellationToken);
    }

    private SecureSocketOptions ResolveSocketOptions()
    {
        if (_settings.SmtpPort == 465)
        {
            return SecureSocketOptions.SslOnConnect;
        }

        return _settings.UseSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None;
    }
}
