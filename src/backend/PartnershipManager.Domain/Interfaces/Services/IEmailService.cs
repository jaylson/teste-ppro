namespace PartnershipManager.Domain.Interfaces.Services;

/// <summary>
/// Interface para serviço de envio de e-mails transacionais
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Envia o e-mail de recuperação de senha com o link de reset.
    /// </summary>
    /// <param name="toEmail">Endereço de destino</param>
    /// <param name="toName">Nome do destinatário</param>
    /// <param name="resetLink">URL completa do reset (ex: https://app.exemplo.com/reset-password?token=...)</param>
    /// <param name="language">Código de idioma do usuário (ex: "pt", "en")</param>
    Task SendPasswordResetEmailAsync(string toEmail, string toName, string resetLink, string language);
}
