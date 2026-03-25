namespace PartnershipManager.Domain.Interfaces.Services;

/// <summary>
/// Interface para serviço de envio de e-mails transacionais
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Envia o e-mail de recuperação de senha com o link de reset.
    /// </summary>
    Task SendPasswordResetEmailAsync(string toEmail, string toName, string resetLink, string language);

    /// <summary>
    /// Envia o e-mail de ativação de conta para novos usuários.
    /// </summary>
    Task SendAccountActivationEmailAsync(string toEmail, string toName, string activationLink, string language);

    /// <summary>
    /// Notifica um usuário sobre uma nova comunicação publicada.
    /// </summary>
    /// <param name="toEmail">E-mail do destinatário</param>
    /// <param name="toName">Nome do destinatário</param>
    /// <param name="commTitle">Título da comunicação</param>
    /// <param name="commSummary">Resumo ou trecho do conteúdo</param>
    /// <param name="commType">Tipo (announcement, update, report, alert, invitation)</param>
    /// <param name="actionUrl">URL para acessar a comunicação</param>
    /// <param name="companyName">Nome da empresa remetente</param>
    Task SendCommunicationPublishedEmailAsync(string toEmail, string toName,
        string commTitle, string? commSummary, string commType,
        string actionUrl, string companyName);

    /// <summary>
    /// Notifica um usuário que ele foi designado como aprovador em um fluxo de aprovação.
    /// </summary>
    /// <param name="toEmail">E-mail do aprovador</param>
    /// <param name="toName">Nome do aprovador</param>
    /// <param name="workflowTitle">Título do fluxo</param>
    /// <param name="stepName">Nome da etapa atribuída</param>
    /// <param name="requesterName">Nome de quem solicitou</param>
    /// <param name="priority">Prioridade (low, medium, high, urgent)</param>
    /// <param name="dueDate">Prazo da etapa (opcional)</param>
    /// <param name="actionUrl">URL para acessar o fluxo</param>
    Task SendApprovalAssignedEmailAsync(string toEmail, string toName,
        string workflowTitle, string stepName, string requesterName,
        string priority, DateTime? dueDate, string actionUrl);

    /// <summary>
    /// Notifica o solicitante sobre a decisão final de um fluxo de aprovação.
    /// </summary>
    /// <param name="toEmail">E-mail do solicitante</param>
    /// <param name="toName">Nome do solicitante</param>
    /// <param name="workflowTitle">Título do fluxo</param>
    /// <param name="finalStatus">Status final: approved ou rejected</param>
    /// <param name="approverName">Nome de quem tomou a decisão</param>
    /// <param name="comments">Comentários do aprovador (opcional)</param>
    /// <param name="actionUrl">URL para acessar o fluxo</param>
    Task SendApprovalDecisionEmailAsync(string toEmail, string toName,
        string workflowTitle, string finalStatus, string approverName,
        string? comments, string actionUrl);
}

