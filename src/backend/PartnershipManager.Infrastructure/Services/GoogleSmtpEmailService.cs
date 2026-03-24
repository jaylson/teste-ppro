using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;
using PartnershipManager.Domain.Entities;
using PartnershipManager.Domain.Interfaces;
using PartnershipManager.Domain.Interfaces.Services;

namespace PartnershipManager.Infrastructure.Services;

/// <summary>
/// Serviço de e-mail via Google Workspace SMTP Relay (smtp-relay.gmail.com:587).
/// Suporta autenticação por credenciais (username/password) ou por IP autorizado
/// (sem credenciais) — conforme configuração no Google Admin Console.
/// </summary>
public sealed class GoogleSmtpEmailService : IEmailService
{
    private readonly IEmailLogRepository _emailLogRepository;
    private readonly ILogger<GoogleSmtpEmailService> _logger;

    private readonly string _smtpHost;
    private readonly int _smtpPort;
    private readonly bool _enableSsl;
    private readonly string? _username;
    private readonly string? _password;
    private readonly string _fromEmail;
    private readonly string _fromName;

    public GoogleSmtpEmailService(
        IConfiguration configuration,
        IEmailLogRepository emailLogRepository,
        ILogger<GoogleSmtpEmailService> logger)
    {
        _emailLogRepository = emailLogRepository;
        _logger = logger;

        var section = configuration.GetSection("Email");
        _smtpHost   = section["SmtpHost"]   ?? "smtp-relay.gmail.com";
        _smtpPort   = int.Parse(section["SmtpPort"] ?? "587");
        _enableSsl  = bool.Parse(section["EnableSsl"]  ?? "true");
        _username   = section["Username"];
        _password   = section["Password"];
        _fromEmail  = section["FromEmail"]  ?? "noreply@example.com";
        _fromName   = section["FromName"]   ?? "Partnership Manager";
    }

    /// <inheritdoc/>
    public async Task SendPasswordResetEmailAsync(
        string toEmail,
        string toName,
        string resetLink,
        string language)
    {
        var logId = Guid.NewGuid();
        var log = new EmailLog
        {
            Id          = logId,
            RecipientEmail = toEmail,
            RecipientName  = toName,
            Subject        = BuildSubject(language),
            TemplateName   = "password-reset",
            ReferenceType  = "password_reset",
            Status         = "queued",
            CreatedAt      = DateTime.UtcNow,
            UpdatedAt      = DateTime.UtcNow
        };

        await _emailLogRepository.CreateAsync(log);

        try
        {
            var message = BuildMessage(toEmail, toName, resetLink, language);
            await SendAsync(message);

            await _emailLogRepository.UpdateStatusAsync(logId, "sent");
            _logger.LogInformation("E-mail de recuperação de senha enviado para {Email}", toEmail);
        }
        catch (Exception ex)
        {
            await _emailLogRepository.UpdateStatusAsync(logId, "failed", ex.Message);
            _logger.LogError(ex, "Falha ao enviar e-mail de recuperação de senha para {Email}", toEmail);
            throw;
        }
    }

    // -----------------------------------------------------------------------
    // Helpers privados
    // -----------------------------------------------------------------------

    private async Task SendAsync(MimeMessage message)
    {
        using var client = new SmtpClient();

        var socketOptions = _enableSsl
            ? SecureSocketOptions.StartTls
            : SecureSocketOptions.None;

        await client.ConnectAsync(_smtpHost, _smtpPort, socketOptions);

        if (!string.IsNullOrWhiteSpace(_username) && !string.IsNullOrWhiteSpace(_password))
        {
            await client.AuthenticateAsync(_username, _password);
        }

        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }

    private MimeMessage BuildMessage(
        string toEmail, string toName, string resetLink, string language)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_fromName, _fromEmail));
        message.To.Add(new MailboxAddress(toName, toEmail));
        message.Subject = BuildSubject(language);

        var bodyBuilder = new BodyBuilder
        {
            HtmlBody = BuildHtmlBody(toName, resetLink, language),
            TextBody = BuildTextBody(toName, resetLink, language)
        };
        message.Body = bodyBuilder.ToMessageBody();

        return message;
    }

    private static string BuildSubject(string language) =>
        language.StartsWith("en", StringComparison.OrdinalIgnoreCase)
            ? "Partnership Manager — Reset your password"
            : "Partnership Manager — Recuperação de senha";

    private static string BuildHtmlBody(string toName, string resetLink, string language)
    {
        bool ptBr = !language.StartsWith("en", StringComparison.OrdinalIgnoreCase);

        string greeting    = ptBr ? $"Olá, {toName}!" : $"Hello, {toName}!";
        string line1       = ptBr
            ? "Recebemos uma solicitação para redefinição da senha da sua conta."
            : "We received a request to reset your account password.";
        string line2       = ptBr
            ? "Clique no botão abaixo para criar uma nova senha. O link é válido por <strong>1 hora</strong>."
            : "Click the button below to create a new password. The link is valid for <strong>1 hour</strong>.";
        string btnText     = ptBr ? "Redefinir Senha" : "Reset Password";
        string line3       = ptBr
            ? "Se você não solicitou isso, ignore este e-mail — sua senha permanece a mesma."
            : "If you did not request this, please ignore this email — your password remains unchanged.";
        string footer      = ptBr
            ? "Este e-mail foi enviado automaticamente. Por favor, não responda."
            : "This is an automated message. Please do not reply.";

        return $@"<!DOCTYPE html>
<html lang=""{(ptBr ? "pt-BR" : "en")}"">
<head>
  <meta charset=""UTF-8"" />
  <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"" />
  <title>{BuildSubject(language)}</title>
</head>
<body style=""margin:0;padding:0;background-color:#f3f4f6;font-family:Arial,Helvetica,sans-serif;"">
  <table width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""background-color:#f3f4f6;padding:40px 0;"">
    <tr>
      <td align=""center"">
        <table width=""600"" cellpadding=""0"" cellspacing=""0"" style=""background-color:#ffffff;border-radius:8px;overflow:hidden;box-shadow:0 2px 8px rgba(0,0,0,0.08);"">
          <!-- Header -->
          <tr>
            <td style=""background-color:#111827;padding:32px 40px;text-align:center;"">
              <span style=""color:#ffffff;font-size:22px;font-weight:bold;letter-spacing:1px;"">Partnership Manager</span>
            </td>
          </tr>
          <!-- Body -->
          <tr>
            <td style=""padding:40px;"">
              <p style=""color:#111827;font-size:18px;font-weight:bold;margin:0 0 16px;"">{greeting}</p>
              <p style=""color:#374151;font-size:15px;line-height:1.6;margin:0 0 12px;"">{line1}</p>
              <p style=""color:#374151;font-size:15px;line-height:1.6;margin:0 0 32px;"">{line2}</p>
              <table width=""100%"" cellpadding=""0"" cellspacing=""0"">
                <tr>
                  <td align=""center"" style=""padding:0 0 32px;"">
                    <a href=""{resetLink}""
                       style=""display:inline-block;background-color:#0891B2;color:#ffffff;font-size:16px;font-weight:bold;text-decoration:none;padding:14px 36px;border-radius:6px;"">
                      {btnText}
                    </a>
                  </td>
                </tr>
              </table>
              <p style=""color:#6b7280;font-size:13px;line-height:1.6;margin:0 0 12px;"">{line3}</p>
              <p style=""color:#9ca3af;font-size:12px;margin:0;"">{footer}</p>
            </td>
          </tr>
          <!-- Footer -->
          <tr>
            <td style=""background-color:#f9fafb;padding:20px 40px;text-align:center;"">
              <p style=""color:#9ca3af;font-size:12px;margin:0;"">© {DateTime.UtcNow.Year} Partnership Manager</p>
            </td>
          </tr>
        </table>
      </td>
    </tr>
  </table>
</body>
</html>";
    }

    private static string BuildTextBody(string toName, string resetLink, string language)
    {
        bool ptBr = !language.StartsWith("en", StringComparison.OrdinalIgnoreCase);

        return ptBr
            ? $"Olá, {toName}!\n\nRecebemos uma solicitação para redefinição da sua senha.\n\nAcesse o link abaixo para criar uma nova senha (válido por 1 hora):\n{resetLink}\n\nSe você não solicitou isso, ignore este e-mail.\n\nPartnership Manager"
            : $"Hello, {toName}!\n\nWe received a request to reset your password.\n\nClick the link below to create a new password (valid for 1 hour):\n{resetLink}\n\nIf you did not request this, please ignore this email.\n\nPartnership Manager";
    }
}
