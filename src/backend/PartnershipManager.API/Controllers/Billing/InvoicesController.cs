using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PartnershipManager.Application.Features.Billing.Commands;
using PartnershipManager.Application.Features.Billing.DTOs;
using PartnershipManager.Application.Features.Billing.Queries;
using PartnershipManager.Domain.Entities.Billing;

namespace PartnershipManager.API.Controllers.Billing;

// [Authorize] // Temporariamente desabilitado para testes
[ApiController]
[Route("api/[controller]")]
public class InvoicesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<InvoicesController> _logger;

    public InvoicesController(
        IMediator mediator,
        ILogger<InvoicesController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Lista todas as faturas com filtros opcionais
    /// </summary>
    /// <param name="clientId">ID do cliente (opcional)</param>
    /// <param name="status">Status da fatura (opcional)</param>
    /// <param name="startDate">Data inicial do período (opcional)</param>
    /// <param name="endDate">Data final do período (opcional)</param>
    /// <param name="planId">ID do plano (opcional)</param>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<InvoiceDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<InvoiceDto>>> GetAll(
        [FromQuery] Guid? clientId = null,
        [FromQuery] string? status = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] Guid? planId = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            InvoiceStatus? invoiceStatus = null;
            if (!string.IsNullOrEmpty(status) && Enum.TryParse<InvoiceStatus>(status, true, out var parsedStatus))
            {
                invoiceStatus = parsedStatus;
            }

            var query = new GetFilteredInvoicesQuery
            {
                ClientId = clientId,
                Status = invoiceStatus,
                StartDate = startDate,
                EndDate = endDate,
                PlanId = planId
            };

            var invoices = await _mediator.Send(query, cancellationToken);
            return Ok(invoices);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar faturas");
            return StatusCode(500, new { message = "Erro ao buscar faturas" });
        }
    }

    /// <summary>
    /// Busca uma fatura por ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(InvoiceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<InvoiceDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var query = new GetInvoiceByIdQuery { Id = id };
            var invoice = await _mediator.Send(query, cancellationToken);

            if (invoice == null)
            {
                return NotFound(new { message = "Fatura não encontrada" });
            }

            return Ok(invoice);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar fatura {InvoiceId}", id);
            return StatusCode(500, new { message = "Erro ao buscar fatura" });
        }
    }

    /// <summary>
    /// Busca faturas de um cliente específico
    /// </summary>
    [HttpGet("client/{clientId}")]
    [ProducesResponseType(typeof(IEnumerable<InvoiceDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<InvoiceDto>>> GetByClient(Guid clientId, CancellationToken cancellationToken)
    {
        try
        {
            var query = new GetInvoicesByClientQuery { ClientId = clientId };
            var invoices = await _mediator.Send(query, cancellationToken);
            return Ok(invoices);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar faturas do cliente {ClientId}", clientId);
            return StatusCode(500, new { message = "Erro ao buscar faturas do cliente" });
        }
    }

    /// <summary>
    /// Cria uma nova fatura
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Guid>> Create([FromBody] CreateInvoiceCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var invoiceId = await _mediator.Send(command, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = invoiceId }, invoiceId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar fatura");
            return StatusCode(500, new { message = "Erro ao criar fatura" });
        }
    }

    /// <summary>
    /// Atualiza uma fatura existente
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Update(Guid id, [FromBody] UpdateInvoiceCommand command, CancellationToken cancellationToken)
    {
        try
        {
            if (id != command.Id)
            {
                return BadRequest(new { message = "ID da URL não corresponde ao ID do corpo da requisição" });
            }

            var result = await _mediator.Send(command, cancellationToken);

            if (!result)
            {
                return NotFound(new { message = "Fatura não encontrada" });
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar fatura {InvoiceId}", id);
            return StatusCode(500, new { message = "Erro ao atualizar fatura" });
        }
    }

    /// <summary>
    /// Marca uma fatura como paga
    /// </summary>
    [HttpPost("{id}/pay")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> MarkAsPaid(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var command = new MarkInvoiceAsPaidCommand { Id = id };
            var result = await _mediator.Send(command, cancellationToken);

            if (!result)
            {
                return NotFound(new { message = "Fatura não encontrada" });
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao marcar fatura {InvoiceId} como paga", id);
            return StatusCode(500, new { message = "Erro ao marcar fatura como paga" });
        }
    }

    /// <summary>
    /// Cancela uma fatura
    /// </summary>
    [HttpPost("{id}/cancel")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Cancel(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var command = new CancelInvoiceCommand { Id = id };
            var result = await _mediator.Send(command, cancellationToken);

            if (!result)
            {
                return NotFound(new { message = "Fatura não encontrada" });
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao cancelar fatura {InvoiceId}", id);
            return StatusCode(500, new { message = "Erro ao cancelar fatura" });
        }
    }

    /// <summary>
    /// Deleta uma fatura
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var command = new DeleteInvoiceCommand { Id = id };
            var result = await _mediator.Send(command, cancellationToken);

            if (!result)
            {
                return NotFound(new { message = "Fatura não encontrada" });
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao deletar fatura {InvoiceId}", id);
            return StatusCode(500, new { message = "Erro ao deletar fatura" });
        }
    }

    /// <summary>
    /// Gera o PDF da fatura
    /// </summary>
    [HttpGet("{id}/pdf")]
    [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> GetPdf(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var query = new GenerateInvoicePdfQuery { InvoiceId = id };
            var pdfBytes = await _mediator.Send(query, cancellationToken);

            if (pdfBytes == null || pdfBytes.Length == 0)
            {
                return NotFound(new { message = "Fatura não encontrada ou erro ao gerar PDF" });
            }

            return File(pdfBytes, "application/pdf", $"invoice-{id}.pdf");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao gerar PDF da fatura {InvoiceId}", id);
            return StatusCode(500, new { message = "Erro ao gerar PDF da fatura" });
        }
    }

    /// <summary>
    /// Obtém estatísticas de faturas
    /// </summary>
    [HttpGet("statistics")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<ActionResult> GetStatistics(
        [FromQuery] Guid? clientId = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var query = new GetInvoiceStatisticsQuery
            {
                ClientId = clientId,
                StartDate = startDate,
                EndDate = endDate
            };

            var statistics = await _mediator.Send(query, cancellationToken);
            return Ok(statistics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar estatísticas de faturas");
            return StatusCode(500, new { message = "Erro ao buscar estatísticas de faturas" });
        }
    }
}
