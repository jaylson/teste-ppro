using System.Globalization;
using PartnershipManager.Domain.Entities.Billing;
using PartnershipManager.Domain.Interfaces.Services;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace PartnershipManager.Infrastructure.Services;

public class PdfGeneratorService : IPdfGeneratorService
{
    public async Task<byte[]> GenerateInvoicePdfAsync(Invoice invoice, CancellationToken cancellationToken = default)
    {
        // Configurar licença QuestPDF (Community License)
        QuestPDF.Settings.License = LicenseType.Community;

        return await Task.Run(() =>
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.DefaultTextStyle(x => x.FontSize(11));

                    page.Header().Element(ComposeHeader);
                    page.Content().Element(container => ComposeContent(container, invoice));
                    page.Footer().AlignCenter().Text(text =>
                    {
                        text.CurrentPageNumber();
                        text.Span(" / ");
                        text.TotalPages();
                    });
                });
            });

            return document.GeneratePdf();
        }, cancellationToken);
    }

    private void ComposeHeader(IContainer container)
    {
        container.Row(row =>
        {
            row.RelativeItem().Column(column =>
            {
                column.Item().Text("PARTNERSHIP MANAGER").FontSize(20).Bold().FontColor(Colors.Blue.Medium);
                column.Item().Text("Sistema de Gestão de Parcerias").FontSize(10).FontColor(Colors.Grey.Darken2);
                column.Item().PaddingTop(5).Text("contato@partnershipmanager.com").FontSize(9);
                column.Item().Text("Tel: +55 (11) 1234-5678").FontSize(9);
            });

            row.ConstantItem(100).AlignRight().Column(column =>
            {
                column.Item().Text("FATURA").FontSize(20).Bold();
                column.Item().Text($"{DateTime.UtcNow:dd/MM/yyyy}").FontSize(9);
            });
        });
    }

    private void ComposeContent(IContainer container, Invoice invoice)
    {
        container.PaddingVertical(20).Column(column =>
        {
            column.Spacing(10);

            // Invoice Info
            column.Item().Row(row =>
            {
                row.RelativeItem().Component(new InvoiceInfoComponent(invoice));
                row.ConstantItem(20);
                row.RelativeItem().Component(new ClientInfoComponent(invoice.Client));
            });

            column.Item().PaddingTop(10).LineHorizontal(1).LineColor(Colors.Grey.Lighten1);

            // Invoice Details
            column.Item().PaddingTop(10).Element(container => ComposeInvoiceDetails(container, invoice));

            column.Item().PaddingTop(10).LineHorizontal(1).LineColor(Colors.Grey.Lighten1);

            // Total
            column.Item().PaddingTop(10).AlignRight().Text(text =>
            {
                text.Span("VALOR TOTAL: ").FontSize(14).Bold();
                text.Span(invoice.Amount.ToString("C", new CultureInfo("pt-BR"))).FontSize(16).Bold().FontColor(Colors.Blue.Medium);
            });

            // Notes
            if (!string.IsNullOrEmpty(invoice.Notes))
            {
                column.Item().PaddingTop(20).BorderColor(Colors.Grey.Lighten2).BorderLeft(3).PaddingLeft(10).Column(noteColumn =>
                {
                    noteColumn.Item().Text("Observações:").FontSize(10).Bold();
                    noteColumn.Item().Text(invoice.Notes).FontSize(9);
                });
            }

            // Payment Instructions
            column.Item().PaddingTop(20).Column(instructionColumn =>
            {
                instructionColumn.Item().Text("Instruções de Pagamento:").FontSize(10).Bold();
                instructionColumn.Item().PaddingTop(5).Text("• Pagamento via transferência bancária ou PIX").FontSize(9);
                instructionColumn.Item().Text("• Enviar comprovante para: financeiro@partnershipmanager.com").FontSize(9);
                instructionColumn.Item().Text("• Manter o número da fatura como referência").FontSize(9);
            });
        });
    }

    private void ComposeInvoiceDetails(IContainer container, Invoice invoice)
    {
        container.Column(column =>
        {
            column.Item().Text("Detalhes da Fatura").FontSize(12).Bold();
            column.Item().PaddingTop(5).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(3);
                    columns.RelativeColumn(2);
                    columns.RelativeColumn(2);
                });

                table.Header(header =>
                {
                    header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Descrição").Bold();
                    header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Período").Bold();
                    header.Cell().Background(Colors.Grey.Lighten3).Padding(5).AlignRight().Text("Valor").Bold();
                });

                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(invoice.Description);
                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text($"{invoice.IssueDate:dd/MM/yyyy} - {invoice.DueDate:dd/MM/yyyy}");
                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).AlignRight().Text(invoice.Amount.ToString("C", new CultureInfo("pt-BR")));
            });
        });
    }

    private class InvoiceInfoComponent : IComponent
    {
        private readonly Invoice _invoice;

        public InvoiceInfoComponent(Invoice invoice)
        {
            _invoice = invoice;
        }

        public void Compose(IContainer container)
        {
            container.Background(Colors.Grey.Lighten4).Padding(10).Column(column =>
            {
                column.Spacing(3);
                column.Item().Text("Informações da Fatura").FontSize(11).Bold();
                column.Item().Text(text =>
                {
                    text.Span("Número: ").FontSize(9);
                    text.Span(_invoice.InvoiceNumber).FontSize(9).Bold();
                });
                column.Item().Text(text =>
                {
                    text.Span("Data de Emissão: ").FontSize(9);
                    text.Span(_invoice.IssueDate.ToString("dd/MM/yyyy")).FontSize(9).Bold();
                });
                column.Item().Text(text =>
                {
                    text.Span("Data de Vencimento: ").FontSize(9);
                    text.Span(_invoice.DueDate.ToString("dd/MM/yyyy")).FontSize(9).Bold();
                });
                column.Item().Text(text =>
                {
                    text.Span("Status: ").FontSize(9);
                    text.Span(GetStatusText(_invoice.Status)).FontSize(9).Bold().FontColor(GetStatusColor(_invoice.Status));
                });
            });
        }

        private string GetStatusText(InvoiceStatus status)
        {
            return status switch
            {
                InvoiceStatus.Pending => "Pendente",
                InvoiceStatus.Paid => "Pago",
                InvoiceStatus.Overdue => "Vencido",
                InvoiceStatus.Cancelled => "Cancelado",
                _ => "Desconhecido"
            };
        }

        private string GetStatusColor(InvoiceStatus status)
        {
            return status switch
            {
                InvoiceStatus.Pending => Colors.Orange.Medium,
                InvoiceStatus.Paid => Colors.Green.Medium,
                InvoiceStatus.Overdue => Colors.Red.Medium,
                InvoiceStatus.Cancelled => Colors.Grey.Medium,
                _ => Colors.Black
            };
        }
    }

    private class ClientInfoComponent : IComponent
    {
        private readonly Client _client;

        public ClientInfoComponent(Client client)
        {
            _client = client;
        }

        public void Compose(IContainer container)
        {
            container.Background(Colors.Grey.Lighten4).Padding(10).Column(column =>
            {
                column.Spacing(3);
                column.Item().Text("Dados do Cliente").FontSize(11).Bold();
                column.Item().Text(_client.Name).FontSize(9).Bold();
                column.Item().Text(_client.Email).FontSize(9);
                column.Item().Text($"Documento: {_client.Document}").FontSize(9);
                if (!string.IsNullOrEmpty(_client.Phone))
                {
                    column.Item().Text($"Telefone: {_client.Phone}").FontSize(9);
                }
            });
        }
    }
}
