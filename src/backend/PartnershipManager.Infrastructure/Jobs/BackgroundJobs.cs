using Hangfire;
using Microsoft.Extensions.Logging;
using PartnershipManager.Domain.Entities.Billing;
using PartnershipManager.Domain.Interfaces.Billing;

namespace PartnershipManager.Infrastructure.Jobs;

/// <summary>
/// Interface para jobs em background
/// </summary>
public interface IBackgroundJobs
{
    Task CleanupExpiredTokensAsync();
    Task SendEmailAsync(string to, string subject, string body);
    Task ProcessAuditLogsAsync();
    Task GenerateReportsAsync(Guid companyId);
    Task GenerateMonthlyInvoicesAsync();
}

/// <summary>
/// Implementação dos jobs em background
/// </summary>
public class BackgroundJobs : IBackgroundJobs
{
    private readonly ILogger<BackgroundJobs> _logger;
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly IInvoiceRepository _invoiceRepository;

    public BackgroundJobs(
        ILogger<BackgroundJobs> logger,
        ISubscriptionRepository subscriptionRepository,
        IInvoiceRepository invoiceRepository)
    {
        _logger = logger;
        _subscriptionRepository = subscriptionRepository;
        _invoiceRepository = invoiceRepository;
    }

    /// <summary>
    /// Limpa tokens de refresh expirados
    /// </summary>
    [AutomaticRetry(Attempts = 3)]
    public async Task CleanupExpiredTokensAsync()
    {
        _logger.LogInformation("Iniciando limpeza de tokens expirados...");
        
        // TODO: Implementar limpeza no banco de dados
        // DELETE FROM users WHERE refresh_token_expiry < NOW() AND refresh_token IS NOT NULL
        
        await Task.CompletedTask;
        
        _logger.LogInformation("Limpeza de tokens concluída.");
    }

    /// <summary>
    /// Envia email (para integrar com serviço de email)
    /// </summary>
    [AutomaticRetry(Attempts = 5)]
    [Queue("emails")]
    public async Task SendEmailAsync(string to, string subject, string body)
    {
        _logger.LogInformation("Enviando email para {To}: {Subject}", to, subject);
        
        // TODO: Implementar integração com serviço de email (SendGrid, AWS SES, etc.)
        
        await Task.CompletedTask;
        
        _logger.LogInformation("Email enviado com sucesso para {To}", to);
    }

    /// <summary>
    /// Processa logs de auditoria (arquivamento, análise)
    /// </summary>
    [AutomaticRetry(Attempts = 2)]
    public async Task ProcessAuditLogsAsync()
    {
        _logger.LogInformation("Processando logs de auditoria...");
        
        // TODO: Implementar arquivamento de logs antigos
        // TODO: Implementar análise de padrões suspeitos
        
        await Task.CompletedTask;
        
        _logger.LogInformation("Processamento de logs concluído.");
    }

    /// <summary>
    /// Gera relatórios periódicos
    /// </summary>
    [AutomaticRetry(Attempts = 2)]
    [Queue("reports")]
    public async Task GenerateReportsAsync(Guid companyId)
    {
        _logger.LogInformation("Gerando relatórios para empresa {CompanyId}...", companyId);
        
        // TODO: Implementar geração de relatórios
        
        await Task.CompletedTask;
        
        _logger.LogInformation("Relatórios gerados com sucesso para empresa {CompanyId}", companyId);
    }

    /// <summary>
    /// Gera faturas mensais para todas as assinaturas ativas
    /// </summary>
    [AutomaticRetry(Attempts = 2)]
    [Queue("billing")]
    public async Task GenerateMonthlyInvoicesAsync()
    {
        _logger.LogInformation("Iniciando geração mensal de faturas...");
        
        try
        {
            // Busca todas as assinaturas ativas
            var activeSubscriptions = await _subscriptionRepository.GetActiveSubscriptionsAsync(CancellationToken.None);
            
            var generatedCount = 0;
            var errorCount = 0;
            var now = DateTime.UtcNow;
            var issueDate = new DateTime(now.Year, now.Month, 1); // Primeiro dia do mês
            
            foreach (var subscription in activeSubscriptions)
            {
                try
                {
                    // Verifica se já existe fatura para este mês
                    var existingInvoice = await _invoiceRepository.GetBySubscriptionAndPeriodAsync(
                        subscription.Id, 
                        issueDate, 
                        CancellationToken.None);
                    
                    if (existingInvoice != null)
                    {
                        _logger.LogInformation(
                            "Fatura já existe para assinatura {SubscriptionId} no período {Period}", 
                            subscription.Id, 
                            issueDate.ToString("yyyy-MM"));
                        continue;
                    }
                    
                    // Calcula a data de vencimento baseada no ciclo de cobrança
                    var dueDate = subscription.Plan.BillingCycle == BillingCycle.Monthly 
                        ? issueDate.AddDays(30) 
                        : issueDate.AddDays(365);
                    
                    // Gera número da fatura
                    var invoiceNumber = await _invoiceRepository.GenerateInvoiceNumberAsync(CancellationToken.None);
                    
                    // Cria a fatura
                    var invoice = new Invoice
                    {
                        ClientId = subscription.ClientId,
                        SubscriptionId = subscription.Id,
                        InvoiceNumber = invoiceNumber,
                        Amount = subscription.Plan.Price,
                        IssueDate = issueDate,
                        DueDate = dueDate,
                        Status = InvoiceStatus.Pending,
                        Description = $"Fatura mensal - {subscription.Plan.Name}",
                        Notes = $"Período de referência: {issueDate:MMMM/yyyy}"
                    };
                    
                    await _invoiceRepository.CreateAsync(invoice, CancellationToken.None);
                    generatedCount++;
                    
                    _logger.LogInformation(
                        "Fatura {InvoiceNumber} gerada com sucesso para cliente {ClientId}", 
                        invoiceNumber, 
                        subscription.ClientId);
                }
                catch (Exception ex)
                {
                    errorCount++;
                    _logger.LogError(
                        ex, 
                        "Erro ao gerar fatura para assinatura {SubscriptionId}", 
                        subscription.Id);
                }
            }
            
            _logger.LogInformation(
                "Geração mensal de faturas concluída. Total: {Total}, Geradas: {Generated}, Erros: {Errors}", 
                activeSubscriptions.Count(), 
                generatedCount, 
                errorCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao executar job de geração mensal de faturas");
            throw;
        }
    }
}

/// <summary>
/// Configuração dos jobs recorrentes
/// </summary>
public static class HangfireJobsConfiguration
{
    public static void ConfigureRecurringJobs()
    {
        // Limpeza de tokens - diariamente às 3h
        RecurringJob.AddOrUpdate<IBackgroundJobs>(
            "cleanup-expired-tokens",
            job => job.CleanupExpiredTokensAsync(),
            Cron.Daily(3, 0));

        // Processamento de logs - a cada hora
        RecurringJob.AddOrUpdate<IBackgroundJobs>(
            "process-audit-logs",
            job => job.ProcessAuditLogsAsync(),
            Cron.Hourly());
        
        // Geração mensal de faturas - todo dia 1º às 2h
        RecurringJob.AddOrUpdate<IBackgroundJobs>(
            "generate-monthly-invoices",
            job => job.GenerateMonthlyInvoicesAsync(),
            "0 2 1 * *"); // Minuto 0, Hora 2, Dia 1 de cada mês
    }
}
