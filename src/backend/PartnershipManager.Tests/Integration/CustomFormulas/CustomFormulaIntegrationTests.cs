using System.Text.Json;
using Moq;
using FluentAssertions;
using PartnershipManager.Application.Features.CustomFormulas.DTOs;
using PartnershipManager.Domain.Entities;
using PartnershipManager.Domain.Interfaces;
using PartnershipManager.Infrastructure.Services.CustomFormulas;
using PartnershipManager.Infrastructure.Services.Valuation;

namespace PartnershipManager.Tests.Integration.CustomFormulas;

/// <summary>
/// F5-TEST-INT-002 — Testa o fluxo completo de fórmulas customizadas:
///   criar fórmula → publicar versão → testar expressão com engine NCalc2 real.
/// Nível de integração: CustomFormulaService + CustomFormulaEngine (NCalc2 real) + repos mockados.
/// </summary>
public class CustomFormulaIntegrationTests
{
    private static readonly Guid ClientId  = Guid.NewGuid();
    private static readonly Guid CompanyId = Guid.NewGuid();
    private static readonly Guid UserId    = Guid.NewGuid();

    private readonly Mock<IUnitOfWork>               _uow;
    private readonly Mock<ICustomFormulaRepository>  _formulasRepo;
    private readonly Mock<IFormulaVersionRepository> _versionsRepo;
    private readonly CustomFormulaEngine             _realEngine;
    private readonly CustomFormulaService            _service;

    public CustomFormulaIntegrationTests()
    {
        _uow          = new Mock<IUnitOfWork>(MockBehavior.Strict);
        _formulasRepo = new Mock<ICustomFormulaRepository>();
        _versionsRepo = new Mock<IFormulaVersionRepository>();
        _realEngine   = new CustomFormulaEngine();

        _uow.SetupGet(x => x.CustomFormulas).Returns(_formulasRepo.Object);
        _uow.SetupGet(x => x.FormulaVersions).Returns(_versionsRepo.Object);
        _uow.Setup(x => x.BeginTransactionAsync()).Returns(Task.CompletedTask);
        _uow.Setup(x => x.CommitTransactionAsync()).Returns(Task.CompletedTask);
        _uow.Setup(x => x.RollbackTransactionAsync()).Returns(Task.CompletedTask);

        _service = new CustomFormulaService(_uow.Object, _realEngine);
    }

    [Fact]
    public async Task CreateAsync_ValidExpression_PersistsFormulaAndFirstVersion()
    {
        var request = new CreateCustomFormulaRequest
        {
            CompanyId   = CompanyId,
            Name        = "Valuation Agro — NPV Simplificado",
            Description = "Fórmula de valuation baseada em receita × múltiplo ajustado",
            SectorTag   = "agro",
            Expression  = "[receita_anual] * [multiplo]",
            Variables   =
            [
                new() { Name = "receita_anual", Label = "Receita Anual", Type = "number" },
                new() { Name = "multiplo",      Label = "Múltiplo",      Type = "number" },
            ],
            ResultUnit  = "BRL",
            ResultLabel = "Valuation",
        };

        ValuationCustomFormula?  capturedFormula = null;
        ValuationFormulaVersion? capturedVersion = null;

        _formulasRepo
            .Setup(x => x.AddAsync(It.IsAny<ValuationCustomFormula>()))
            .Callback<ValuationCustomFormula>(f => capturedFormula = f)
            .Returns(Task.CompletedTask);

        _versionsRepo
            .Setup(x => x.AddAsync(It.IsAny<ValuationFormulaVersion>()))
            .Callback<ValuationFormulaVersion>(v => capturedVersion = v)
            .Returns(Task.CompletedTask);

        _formulasRepo
            .Setup(x => x.UpdateAsync(It.IsAny<ValuationCustomFormula>()))
            .Returns(Task.CompletedTask);

        var result = await _service.CreateAsync(ClientId, request, UserId);

        result.Should().NotBeNull();
        result.Name.Should().Be(request.Name);
        result.ClientId.Should().Be(ClientId);

        capturedFormula.Should().NotBeNull("fórmula deve ser persistida no repositório");
        capturedVersion.Should().NotBeNull("versão inicial deve ser criada automaticamente");
        capturedVersion!.VersionNumber.Should().Be(1, "primeira versão deve ter VersionNumber = 1");
        capturedVersion.Expression.Should().Be("[receita_anual] * [multiplo]");
        capturedVersion.IsValidated.Should().BeTrue("expressão válida deve ser marcada como validada");

        _formulasRepo.Verify(x => x.AddAsync(It.IsAny<ValuationCustomFormula>()), Times.Once);
        _versionsRepo.Verify(x => x.AddAsync(It.IsAny<ValuationFormulaVersion>()), Times.Once);
        _formulasRepo.Verify(x => x.UpdateAsync(It.IsAny<ValuationCustomFormula>()), Times.Once);
    }

    [Fact]
    public async Task TestFormulaAsync_ValidExpression_ReturnsCorrectResult()
    {
        var request = new TestFormulaRequest
        {
            Expression = "[hectares] * [preco_saca] * [sacas_por_hectare]",
            Inputs = new Dictionary<string, decimal>
            {
                ["hectares"]          = 500m,
                ["preco_saca"]        = 45m,
                ["sacas_por_hectare"] = 65m,
            },
        };

        var result = await _service.TestFormulaAsync(request);

        result.Should().NotBeNull();
        result.IsValid.Should().BeTrue("expressão aritmética correta deve ser válida");
        result.Result.Should().Be(1_462_500m, "500 ha × R$45/saca × 65 sacas/ha = R$ 1.462.500");
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task TestFormulaAsync_BlockedKeyword_ReturnsInvalidResult()
    {
        var request = new TestFormulaRequest
        {
            Expression = "System.Console.WriteLine('hack')",
            Inputs     = new Dictionary<string, decimal>(),
        };

        var result = await _service.TestFormulaAsync(request);

        result.IsValid.Should().BeFalse("keyword bloqueada não deve ser aceita");
        result.Errors.Should().NotBeEmpty("deve descrever o motivo da rejeição");
        result.Result.Should().BeNull();
    }

    [Fact]
    public async Task PublishNewVersionAsync_ValidRequest_IncrementsVersionNumber()
    {
        var formulaId      = Guid.NewGuid();
        var existingFormula = ValuationCustomFormula.Create(
            ClientId, CompanyId, "Fórmula SaaS", null, null, UserId);

        ValuationFormulaVersion? capturedNewVersion = null;

        _formulasRepo
            .Setup(x => x.GetByIdAsync(formulaId, ClientId))
            .ReturnsAsync(existingFormula);
        _versionsRepo
            .Setup(x => x.GetNextVersionNumberAsync(formulaId))
            .ReturnsAsync(2);
        _versionsRepo
            .Setup(x => x.AddAsync(It.IsAny<ValuationFormulaVersion>()))
            .Callback<ValuationFormulaVersion>(v => capturedNewVersion = v)
            .Returns(Task.CompletedTask);
        _formulasRepo
            .Setup(x => x.UpdateAsync(It.IsAny<ValuationCustomFormula>()))
            .Returns(Task.CompletedTask);

        var publishRequest = new PublishNewFormulaVersionRequest
        {
            Expression = "[arr] * [multiple] - [operating_expenses]",
            Variables  = [],
            ResultUnit = "BRL",
        };

        var result = await _service.PublishNewVersionAsync(formulaId, ClientId, publishRequest, UserId);

        result.Should().NotBeNull();
        capturedNewVersion.Should().NotBeNull("nova versão deve ser criada");
        capturedNewVersion!.VersionNumber.Should().Be(2, "segunda publicação deve ter VersionNumber = 2");
        capturedNewVersion.Expression.Should().Contain("[arr]");
        capturedNewVersion.IsValidated.Should().BeTrue("expressão NCalc2 válida deve ser marcada como validada");

        _versionsRepo.Verify(x => x.AddAsync(It.IsAny<ValuationFormulaVersion>()), Times.Once);
        _formulasRepo.Verify(x => x.UpdateAsync(It.IsAny<ValuationCustomFormula>()), Times.Once);
    }

    [Theory]
    [InlineData("[a] + [b]",                             3, 4,           0, 7)]
    [InlineData("[receita] * [multiplo]",          1_000_000, 8,          0, 8_000_000)]
    [InlineData("([ebitda] / [receita]) * 100",     200_000, 1_000_000,  0, 20)]
    public void RealEngine_Evaluate_ProducesCorrectResults(
        string expression, decimal v1, decimal v2, decimal v3, decimal expected)
    {
        var varNames = System.Text.RegularExpressions.Regex
            .Matches(expression, @"\[(\w+)\]")
            .Select(m => m.Groups[1].Value)
            .Distinct()
            .ToArray();

        var rawValues = new[] { v1, v2, v3 };
        var inputs = varNames
            .Select((name, i) => (name, rawValues[i]))
            .ToDictionary(x => x.name, x => x.Item2);

        var result = _realEngine.Evaluate(expression, inputs);

        result.Should().BeApproximately(expected, 0.001m);
    }
}
