using PartnershipManager.Application.Features.Valuation.DTOs;
using PartnershipManager.Infrastructure.Services.Valuation;
using ValuationEntity = PartnershipManager.Domain.Entities.Valuation;
using ValuationMethodEntity = PartnershipManager.Domain.Entities.ValuationMethod;

namespace PartnershipManager.Tests.Integration.Valuation;

/// <summary>
/// F5-TEST-INT-001 — Testa o fluxo completo de aprovação de valuation:
///   draft → submit → approve → verifica PricePerShare (VA-04)
/// Nível de integração: serviço + domínio (repositórios mockados).
/// </summary>
public class ValuationWorkflowIntegrationTests
{
    // ─── Fixtures ─────────────────────────────────────────────────────────────

    private static readonly Guid ClientId  = Guid.NewGuid();
    private static readonly Guid CompanyId = Guid.NewGuid();
    private static readonly Guid UserId    = Guid.NewGuid();

    private readonly Mock<IUnitOfWork>                   _uow;
    private readonly Mock<IValuationRepository>          _valuationsRepo;
    private readonly Mock<IValuationMethodRepository>    _methodsRepo;
    private readonly Mock<IFormulaVersionRepository>     _versionsRepo;
    private readonly Mock<IValuationCalculationEngine>   _calcEngine;
    private readonly ValuationService                    _service;

    public ValuationWorkflowIntegrationTests()
    {
        _uow            = new Mock<IUnitOfWork>(MockBehavior.Strict);
        _valuationsRepo = new Mock<IValuationRepository>();
        _methodsRepo    = new Mock<IValuationMethodRepository>();
        _versionsRepo   = new Mock<IFormulaVersionRepository>();
        _calcEngine     = new Mock<IValuationCalculationEngine>();

        _uow.SetupGet(x => x.Valuations).Returns(_valuationsRepo.Object);
        _uow.SetupGet(x => x.ValuationMethods).Returns(_methodsRepo.Object);
        _uow.SetupGet(x => x.FormulaVersions).Returns(_versionsRepo.Object);

        _service = new ValuationService(_uow.Object, _calcEngine.Object);
    }

    // ─── F5-INT-VAL-001: Workflow completo ────────────────────────────────────

    [Fact]
    public async Task FullWorkflow_DraftToApprove_ComputesPricePerShareCorrectly()
    {
        // STEP 1 — Criar valuation (draft)
        var createRequest = new CreateValuationRequest
        {
            CompanyId   = CompanyId,
            ValuationDate = DateTime.UtcNow,
            EventType   = "series_a",
            EventName   = "Series A Round",
            TotalShares = 1_000_000m,
            Notes       = "Rodada Series A 2026",
        };

        _valuationsRepo
            .Setup(x => x.AddAsync(It.IsAny<ValuationEntity>()))
            .Returns(Task.CompletedTask);

        var draft = await _service.CreateAsync(ClientId, createRequest, UserId);

        draft.Status.Should().Be("draft", "valuation criado deve estar em rascunho");
        draft.CompanyId.Should().Be(CompanyId);
        draft.TotalShares.Should().Be(1_000_000);

        // STEP 2 — Adicionar metodologia ARR Multiple com valor calculado
        var valuationId = draft.Id;
        var valuationEntity = ValuationEntity.Create(
            ClientId, CompanyId, DateTime.UtcNow, "series_a", 1_000_000m, createdBy: UserId);

        var method = ValuationMethodEntity.Create(
            ClientId, valuationId,
            methodType: "arr_multiple",
            inputsJson: """{"arr":5000000,"multiple":10}""",
            createdBy: UserId);
        method.SetCalculatedValue(50_000_000m, UserId);
        method.Select(UserId);

        _valuationsRepo
            .Setup(x => x.GetByIdAsync(valuationId, ClientId))
            .ReturnsAsync(valuationEntity);
        _methodsRepo
            .Setup(x => x.AddAsync(It.IsAny<ValuationMethodEntity>()))
            .Returns(Task.CompletedTask);
        _methodsRepo
            .Setup(x => x.GetByValuationAsync(valuationId, ClientId))
            .ReturnsAsync(new[] { method });

        var addRequest = new AddValuationMethodRequest
        {
            MethodType  = "arr_multiple",
            InputsJson  = """{"arr":5000000,"multiple":10}""",
            DataSource  = "SaaS benchmark 2026",
        };
        var addedMethod = await _service.AddMethodAsync(valuationId, ClientId, addRequest, UserId);
        addedMethod.Should().NotBeNull("metodologia deve ser persistida");

        // STEP 3 — Submit para aprovação
        valuationEntity.SetValuationAmount(50_000_000m, UserId);
        _methodsRepo
            .Setup(x => x.GetSelectedAsync(valuationId, ClientId))
            .ReturnsAsync(method);
        _valuationsRepo
            .Setup(x => x.UpdateAsync(It.IsAny<ValuationEntity>()))
            .Returns(Task.CompletedTask);

        var submitted = await _service.SubmitAsync(valuationId, ClientId, UserId);
        submitted.Status.Should().Be("pending_approval", "após submit deve estar aguardando aprovação");

        // STEP 4 — Aprovar valuation (VA-04: PricePerShare = ValuationAmount / TotalShares)
        // Rebuild entity with pending_approval state
        var pendingEntity = ValuationEntity.Create(
            ClientId, CompanyId, DateTime.UtcNow, "series_a", 1_000_000m, createdBy: UserId);
        pendingEntity.SetValuationAmount(50_000_000m, UserId);
        pendingEntity.Submit(UserId);

        _valuationsRepo
            .Setup(x => x.GetByIdAsync(valuationId, ClientId))
            .ReturnsAsync(pendingEntity);

        var approved = await _service.ApproveAsync(valuationId, ClientId, UserId);

        // STEP 5 — Verificações de integridade (VA-04)
        approved.Status.Should().Be("approved", "valuation deve estar aprovado");
        approved.ValuationAmount.Should().Be(50_000_000m);
        approved.PricePerShare.Should().NotBeNull("VA-04: PricePerShare deve ser calculado na aprovação");
        approved.PricePerShare.Should()
            .BeApproximately(50m, precision: 0.0001m, "VA-04: R$ 50_000_000 / 1_000_000 ações = R$ 50,00/ação");
        approved.ApprovedAt.Should().NotBeNull("data de aprovação deve ser registrada");
        _valuationsRepo.Verify(x => x.UpdateAsync(It.IsAny<ValuationEntity>()), Times.AtLeastOnce);
    }

    // ─── F5-INT-VAL-002: Valuation com fórmula customizada seleciona PricePerShare ──

    [Fact]
    public async Task ApproveAsync_CustomMethodSelected_ReturnsPricePerShare()
    {
        const decimal customValue = 80_000_000m;
        const decimal totalShares = 2_000_000m;
        decimal expectedPps = Math.Round(customValue / totalShares, 6); // = 40

        var entity = ValuationEntity.Create(
            ClientId, CompanyId, DateTime.UtcNow, "seed", totalShares, createdBy: UserId);
        entity.SetValuationAmount(customValue, UserId);
        entity.Submit(UserId);

        var valuationId = entity.Id;
        _valuationsRepo
            .Setup(x => x.GetByIdAsync(valuationId, ClientId))
            .ReturnsAsync(entity);
        _valuationsRepo
            .Setup(x => x.UpdateAsync(It.IsAny<ValuationEntity>()))
            .Returns(Task.CompletedTask);
        _methodsRepo
            .Setup(x => x.GetByValuationAsync(valuationId, ClientId))
            .ReturnsAsync(Enumerable.Empty<ValuationMethodEntity>());

        var result = await _service.ApproveAsync(valuationId, ClientId, UserId);

        result.Status.Should().Be("approved");
        result.PricePerShare.Should()
            .BeApproximately(expectedPps, 0.0001m, $"VA-04: R$ {customValue:N0} / {totalShares:N0} = R$ {expectedPps}");
    }

    // ─── F5-INT-VAL-003: Submit sem metodologia selecionada lança exceção ─────

    [Fact]
    public async Task SubmitAsync_NoSelectedMethod_ThrowsDomainException()
    {
        var entity = ValuationEntity.Create(
            ClientId, CompanyId, DateTime.UtcNow, "internal", 5_000m, createdBy: UserId);

        _valuationsRepo
            .Setup(x => x.GetByIdAsync(entity.Id, ClientId))
            .ReturnsAsync(entity);
        _methodsRepo
            .Setup(x => x.GetSelectedAsync(entity.Id, ClientId))
            .ReturnsAsync((ValuationMethodEntity?)null);

        var act = async () => await _service.SubmitAsync(entity.Id, ClientId, UserId);
        await act.Should().ThrowAsync<Exception>("submetendo sem metodologia deve lançar exceção de domínio");
    }

    // ─── F5-INT-VAL-004: Double-approve deve lançar exceção ───────────────────

    [Fact]
    public async Task ApproveAsync_AlreadyApproved_ThrowsDomainException()
    {
        var entity = ValuationEntity.Create(
            ClientId, CompanyId, DateTime.UtcNow, "series_b", 10_000m, createdBy: UserId);
        entity.SetValuationAmount(100_000_000m, UserId);
        entity.Submit(UserId);
        entity.Approve(UserId); // já aprovado

        _valuationsRepo
            .Setup(x => x.GetByIdAsync(entity.Id, ClientId))
            .ReturnsAsync(entity);

        var act = async () => await _service.ApproveAsync(entity.Id, ClientId, UserId);
        await act.Should().ThrowAsync<Exception>("re-aprovar valuation já aprovado deve falhar com exceção");
    }
}
