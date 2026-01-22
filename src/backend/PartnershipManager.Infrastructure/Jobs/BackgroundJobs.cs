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
    Task GenerateMonthlyInvoicesAsync(int month, int year);
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
    public async Task GenerateMonthlyInvoicesAsync(int month, int year)
    {
        _logger.LogInformation("Iniciando geração mensal de faturas para {Month}/{Year}...", month, year);
        
        var logEntries = new List<string>();
        var now = DateTime.UtcNow;
        var issueDate = new DateTime(year, month, 1);
        
        logEntries.Add($"=== RELATÓRIO DE GERAÇÃO DE FATURAS ===");
        logEntries.Add($"Data/Hora: {now:dd/MM/yyyy HH:mm:ss} UTC");
        logEntries.Add($"Período de referência: {issueDate:MMMM/yyyy}");
        logEntries.Add($"Primeira execução do mês: {issueDate:dd/MM/yyyy}");
        logEntries.Add("");
        
        try
        {
            // Busca todas as assinaturas e filtra as ativas
            var allSubscriptions = await _subscriptionRepository.GetAllAsync(CancellationToken.None);
            var activeSubscriptions = allSubscriptions.Where(s => s.Status == SubscriptionStatus.Active).ToList();
            
            logEntries.Add($"Total de assinaturas no sistema: {allSubscriptions.Count()}");
            logEntries.Add($"Assinaturas ativas: {activeSubscriptions.Count}");
            logEntries.Add($"Assinaturas inativas: {allSubscriptions.Count() - activeSubscriptions.Count}");
            logEntries.Add("");
            logEntries.Add("--- PROCESSAMENTO DE ASSINATURAS ---");
            logEntries.Add("");
            
            var generatedCount = 0;
            var errorCount = 0;
            var skippedCount = 0;
            
            foreach (var subscription in activeSubscriptions)
            {
                try
                {
                    var clientName = subscription.Client?.Name ?? "Cliente não identificado";
                    var planName = subscription.Plan?.Name ?? "Plano não identificado";
                    var planPrice = subscription.Plan?.Price ?? 0;
                    
                    logEntries.Add($"[Assinatura #{subscription.Id}]");
                    logEntries.Add($"  Cliente: {clientName}");
                    logEntries.Add($"  Plano: {planName} (R$ {planPrice:N2})");
                    logEntries.Add($"  Status: {subscription.Status}");
                    
                    // Verifica se já existe fatura para este mês e assinatura
                    var allInvoices = await _invoiceRepository.GetAllAsync(CancellationToken.None);
                    var existingInvoice = allInvoices.FirstOrDefault(i => 
                        i.SubscriptionId == subscription.Id && 
                        i.IssueDate.Year == issueDate.Year && 
                        i.IssueDate.Month == issueDate.Month &&
                        i.Status != InvoiceStatus.Cancelled);
                    
                    if (existingInvoice != null)
                    {
                        skippedCount++;
                        logEntries.Add($"  ❌ PULADA: Fatura já existe - #{existingInvoice.InvoiceNumber}");
                        logEntries.Add($"  Motivo: Fatura já gerada para este período");
                        logEntries.Add($"  Data da fatura existente: {existingInvoice.IssueDate:dd/MM/yyyy}");
                        logEntries.Add("");
                        
                        _logger.LogInformation(
                            "Fatura já existe para assinatura {SubscriptionId} no período {Period} - Fatura #{InvoiceNumber}", 
                            subscription.Id, 
                            issueDate.ToString("yyyy-MM"),
                            existingInvoice.InvoiceNumber);
                        continue;
                    }
                    
                    // Calcula a data de vencimento: dia de vencimento da assinatura no mês seguinte ao período de geração
                    var nextMonth = issueDate.AddMonths(1);
                    var maxDayInNextMonth = DateTime.DaysInMonth(nextMonth.Year, nextMonth.Month);
                    var dueDay = Math.Min(subscription.DueDay, maxDayInNextMonth); // Ajusta para meses com menos dias
                    var dueDate = new DateTime(nextMonth.Year, nextMonth.Month, dueDay);
                    
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
                    
                    logEntries.Add($"  ✅ GERADA COM SUCESSO: #{invoiceNumber}");
                    logEntries.Add($"  Valor: R$ {invoice.Amount:N2}");
                    logEntries.Add($"  Data de emissão: {invoice.IssueDate:dd/MM/yyyy}");
                    logEntries.Add($"  Data de vencimento: {invoice.DueDate:dd/MM/yyyy}");
                    logEntries.Add("");
                    
                    _logger.LogInformation(
                        "Fatura {InvoiceNumber} gerada com sucesso para cliente {ClientId}", 
                        invoiceNumber, 
                        subscription.ClientId);
                }
                catch (Exception ex)
                {
                    errorCount++;
                    var clientName = subscription.Client?.Name ?? "Cliente não identificado";
                    logEntries.Add($"  ❌ ERRO: Falha ao gerar fatura");
                    logEntries.Add($"  Motivo: {ex.Message}");
                    logEntries.Add($"  Tipo de erro: {ex.GetType().Name}");
                    logEntries.Add("");
                    
                    _logger.LogError(
                        ex, 
                        "Erro ao gerar fatura para assinatura {SubscriptionId}", 
                        subscription.Id);
                }
            }
            
            // Resumo final
            logEntries.Add("\n=== RESUMO ===");
            logEntries.Add($"Total processado: {activeSubscriptions.Count}");
            logEntries.Add($"✅ Geradas com sucesso: {generatedCount}");
            logEntries.Add($"⏭️  Puladas (já existentes): {skippedCount}");
            logEntries.Add($"❌ Erros: {errorCount}");
            logEntries.Add($"\nProcessamento concluído em: {DateTime.UtcNow:dd/MM/yyyy HH:mm:ss} UTC");
            
            // Salvar log em arquivo
            await SaveInvoiceGenerationLog(logEntries, issueDate);
            
            _logger.LogInformation(
                "Geração de faturas concluída. Geradas: {GeneratedCount}, Puladas: {SkippedCount}, Erros: {ErrorCount}", 
                generatedCount, 
                skippedCount,
                errorCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro fatal ao gerar faturas mensais");
            
            // Tentar salvar log de erro
            try
            {
                var errorLog = new List<string>
                {
                    "=== ERRO FATAL NA GERAÇÃO DE FATURAS ===",
                    $"Data/Hora: {DateTime.UtcNow:dd/MM/yyyy HH:mm:ss} UTC",
                    $"Erro: {ex.Message}",
                    $"Stack Trace: {ex.StackTrace}"
                };
                await SaveInvoiceGenerationLog(errorLog, issueDate);
            }
            catch
            {
                // Ignorar erro ao salvar log de erro
            }
            
            throw;
        }
    }
    
    private async Task SaveInvoiceGenerationLog(List<string> logEntries, DateTime issueDate)
    {
        try
        {
            var logDirectory = "/app/logs/billing";
            Directory.CreateDirectory(logDirectory);
            
            var fileName = $"invoice-generation-{issueDate:yyyy-MM}-{DateTime.UtcNow:yyyyMMdd-HHmmss}.log";
            var filePath = Path.Combine(logDirectory, fileName);
            
            await File.WriteAllLinesAsync(filePath, logEntries);
            
            _logger.LogInformation(
                "Log de geração de faturas salvo em: {FilePath}", 
                filePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao salvar log de geração de faturas");
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
            job => job.GenerateMonthlyInvoicesAsync(DateTime.UtcNow.Month, DateTime.UtcNow.Year),
            "0 2 1 * *"); // Minuto 0, Hora 2, Dia 1 de cada mês
    }
}
