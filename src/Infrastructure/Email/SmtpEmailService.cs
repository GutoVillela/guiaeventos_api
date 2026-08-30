using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Shared.Email;

namespace Infrastructure.Email;

public class SmtpEmailService : IEmailService
{
    private readonly IConfiguration _config;
    private readonly ILogger<SmtpEmailService> _logger;

    public SmtpEmailService(IConfiguration config, ILogger<SmtpEmailService> logger)
    {
        _config = config;
        _logger = logger;
    }

    public async Task SendAsync(string toEmail, string toName, string subject, string htmlBody, CancellationToken ct = default)
    {
        var host = _config["Email:Host"];
        var port = int.Parse(_config["Email:Port"] ?? "587");
        var enableSsl = bool.Parse(_config["Email:EnableSsl"] ?? "true");
        var username = _config["Email:Username"];
        var password = _config["Email:Password"];
        var fromAddress = _config["Email:FromAddress"] ?? username;
        var fromName = _config["Email:FromName"] ?? "Guia Evento Escolar";

        if (string.IsNullOrEmpty(host) || string.IsNullOrEmpty(username))
        {
            _logger.LogWarning("Email service not configured. Skipping send to {Email}. Subject: {Subject}", toEmail, subject);
            _logger.LogInformation("[EMAIL MOCK] To: {To} | Subject: {Subject} | Body preview: {Body}",
                toEmail, subject, htmlBody.Length > 200 ? htmlBody[..200] + "..." : htmlBody);
            return;
        }

        using var client = new SmtpClient(host, port)
        {
            EnableSsl = enableSsl,
            Credentials = new NetworkCredential(username, password),
            DeliveryMethod = SmtpDeliveryMethod.Network,
        };

        using var message = new MailMessage
        {
            From = new MailAddress(fromAddress!, fromName),
            Subject = subject,
            Body = htmlBody,
            IsBodyHtml = true,
        };
        message.To.Add(new MailAddress(toEmail, toName));

        await client.SendMailAsync(message, ct);
        _logger.LogInformation("Email sent to {Email} with subject: {Subject}", toEmail, subject);
    }
}
