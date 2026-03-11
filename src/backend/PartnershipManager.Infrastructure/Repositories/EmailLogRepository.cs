using Dapper;
using PartnershipManager.Domain.Entities;
using PartnershipManager.Domain.Interfaces;
using PartnershipManager.Infrastructure.Persistence;

namespace PartnershipManager.Infrastructure.Repositories;

public class EmailLogRepository : IEmailLogRepository
{
    private readonly DapperContext _context;

    public EmailLogRepository(DapperContext context)
    {
        _context = context;
    }

    public async Task<Guid> CreateAsync(EmailLog log)
    {
        var sql = @"
            INSERT INTO email_logs (id, company_id, recipient_email, recipient_name, subject, template_name,
                reference_type, reference_id, resend_message_id, status, error_message, sent_at, created_at, updated_at)
            VALUES (@Id, @CompanyId, @RecipientEmail, @RecipientName, @Subject, @TemplateName,
                @ReferenceType, @ReferenceId, @ResendMessageId, @Status, @ErrorMessage, @SentAt, @CreatedAt, @UpdatedAt)";
        await _context.Connection.ExecuteAsync(sql, log);
        return log.Id;
    }

    public async Task UpdateStatusAsync(Guid id, string status, string? errorMessage = null, string? resendMessageId = null)
    {
        var sql = @"UPDATE email_logs SET status = @Status, error_message = @ErrorMessage,
                    resend_message_id = @ResendMessageId, sent_at = @SentAt, updated_at = @Now
                    WHERE id = @Id";
        await _context.Connection.ExecuteAsync(sql, new
        {
            Id = id,
            Status = status,
            ErrorMessage = errorMessage,
            ResendMessageId = resendMessageId,
            SentAt = status == "sent" ? DateTime.UtcNow : (DateTime?)null,
            Now = DateTime.UtcNow
        });
    }
}
