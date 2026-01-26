using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PartnershipManager.API.Middlewares;
using PartnershipManager.Application.Common.Models;
using PartnershipManager.Application.Features.Simulation.DTOs;
using PartnershipManager.Infrastructure.Services;

namespace PartnershipManager.API.Controllers;

/// <summary>
/// Simulador de rodadas de investimento
/// </summary>
[ApiController]
[Route("api/simulator")]
[Authorize]
[Produces("application/json")]
public class SimulatorController : ControllerBase
{
    private readonly IRoundSimulatorService _simulatorService;
    private readonly ILogger<SimulatorController> _logger;

    public SimulatorController(IRoundSimulatorService simulatorService, ILogger<SimulatorController> logger)
    {
        _simulatorService = simulatorService;
        _logger = logger;
    }

    /// <summary>
    /// Simula uma rodada de investimento
    /// </summary>
    /// <remarks>
    /// Calcula o impacto de uma nova rodada de investimento no cap table,
    /// incluindo diluição de acionistas existentes, novas participações
    /// e opcionalmente um pool de opções.
    /// 
    /// Exemplo de request:
    /// ```json
    /// {
    ///   "companyId": "guid",
    ///   "preMoneyValuation": 10000000,
    ///   "investmentAmount": 2000000,
    ///   "roundName": "Series A",
    ///   "roundType": 1,
    ///   "newInvestors": [
    ///     { "name": "Fundo ABC", "investmentAmount": 1500000 },
    ///     { "name": "Angel XYZ", "investmentAmount": 500000 }
    ///   ],
    ///   "includeOptionPool": true,
    ///   "optionPoolPercentage": 10,
    ///   "optionPoolPreMoney": true
    /// }
    /// ```
    /// </remarks>
    /// <param name="request">Parâmetros da simulação</param>
    /// <returns>Resultado da simulação com cap table antes e depois</returns>
    [HttpPost("round")]
    [ProducesResponseType(typeof(ApiResponse<RoundSimulationResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SimulateRound([FromBody] RoundSimulationRequest request)
    {
        var clientId = HttpContext.GetRequiredClientId();

        _logger.LogInformation(
            "Simulando rodada {RoundName} para empresa {CompanyId}: Pre-money {PreMoney}, Investment {Investment}",
            request.RoundName,
            request.CompanyId,
            request.PreMoneyValuation,
            request.InvestmentAmount);

        var result = await _simulatorService.SimulateRoundAsync(clientId, request);

        return Ok(ApiResponse<RoundSimulationResponse>.Ok(result, "Simulação realizada com sucesso"));
    }

    /// <summary>
    /// Calcula a diluição de uma rodada
    /// </summary>
    /// <remarks>
    /// Retorna apenas o percentual de diluição, sem detalhes do cap table.
    /// Útil para cálculos rápidos.
    /// </remarks>
    /// <param name="companyId">ID da empresa</param>
    /// <param name="investmentAmount">Valor do investimento</param>
    /// <param name="preMoneyValuation">Valuation pre-money</param>
    /// <returns>Percentual de diluição</returns>
    [HttpGet("dilution")]
    [ProducesResponseType(typeof(ApiResponse<DilutionResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CalculateDilution(
        [FromQuery] Guid companyId,
        [FromQuery] decimal investmentAmount,
        [FromQuery] decimal preMoneyValuation)
    {
        var clientId = HttpContext.GetRequiredClientId();

        var dilution = await _simulatorService.CalculateDilutionAsync(
            clientId, 
            companyId, 
            investmentAmount, 
            preMoneyValuation);

        var response = new DilutionResponse
        {
            CompanyId = companyId,
            InvestmentAmount = investmentAmount,
            PreMoneyValuation = preMoneyValuation,
            PostMoneyValuation = preMoneyValuation + investmentAmount,
            DilutionPercentage = dilution
        };

        return Ok(ApiResponse<DilutionResponse>.Ok(response));
    }

    /// <summary>
    /// Simula múltiplos cenários de rodada
    /// </summary>
    /// <remarks>
    /// Permite comparar diferentes cenários de valuation e investimento.
    /// </remarks>
    [HttpPost("scenarios")]
    [ProducesResponseType(typeof(ApiResponse<List<RoundSimulationResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SimulateScenarios([FromBody] List<RoundSimulationRequest> requests)
    {
        var clientId = HttpContext.GetRequiredClientId();

        if (requests == null || requests.Count == 0)
        {
            return BadRequest(ApiResponse<object>.Error("Pelo menos um cenário deve ser fornecido"));
        }

        if (requests.Count > 5)
        {
            return BadRequest(ApiResponse<object>.Error("Máximo de 5 cenários por vez"));
        }

        var results = new List<RoundSimulationResponse>();

        foreach (var request in requests)
        {
            var result = await _simulatorService.SimulateRoundAsync(clientId, request);
            results.Add(result);
        }

        return Ok(ApiResponse<List<RoundSimulationResponse>>.Ok(results, $"{results.Count} cenários simulados"));
    }
}

/// <summary>
/// Resposta simplificada de diluição
/// </summary>
public record DilutionResponse
{
    public Guid CompanyId { get; init; }
    public decimal InvestmentAmount { get; init; }
    public decimal PreMoneyValuation { get; init; }
    public decimal PostMoneyValuation { get; init; }
    public decimal DilutionPercentage { get; init; }
}
