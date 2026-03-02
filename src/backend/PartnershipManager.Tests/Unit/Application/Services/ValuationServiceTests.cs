using PartnershipManager.Application.Features.Valuation.DTOs;
using PartnershipManager.Domain.Exceptions;
using PartnershipManager.Infrastructure.Services.Valuation;
using ValuationEntity = PartnershipManager.Domain.Entities.Valuation;
using ValuationMethodEntity = PartnershipManager.Domain.Entities.ValuationMethod;

namespace PartnershipManager.Tests.Unit.Application.Services;

public class ValuationServiceTests
{
    private readonly Mock<IUnitOfWork> _uow;
    private readonly Mock<IValuationRepository> _valuationsRepo;
    private readonly Mock<IValuationMethodRepository> _methodsRepo;
    private readonly Mock<IFormulaVersionRepository> _versionsRepo;
    private readonly Mock<IValuationCalculationEngine> _calcEngine;
    private readonly ValuationService _service;

    private static readonly Guid ClientId = Guid.NewGuid();
    private static readonly Guid CompanyId = Guid.NewGuid();
    private static readonly Guid UserId = Guid.NewGuid();

    public ValuationServiceTests()
    {
        _uow = new Mock<IUnitOfWork>();
        _valuationsRepo = new Mock<IValuationRepository>();
        _methodsRepo = new Mock<IValuationMethodRepository>();
        _versionsRepo = new Mock<IFormulaVersionRepository>();
        _calcEngine = new Mock<IValuationCalculationEngine>();

        _uow.SetupGet(x => x.Valuations).Returns(_valuationsRepo.Object);
        _uow.SetupGet(x => x.ValuationMethods).Returns(_methodsRepo.Object);
        _uow.SetupGet(x => x.FormulaVersions).Returns(_versionsRepo.Object);

        _service = new ValuationService(_uow.Object, _calcEngine.Object);
    }

    // ─── GetByIdAsync ─────────────────────────────────────────────────────────

    [Fact]
    public async Task GetByIdAsync_ExistingId_ReturnsValuationResponse()
    {
        var valuation = MakeValuation();
        _valuationsRepo.Setup(x => x.GetByIdAsync(valuation.Id, ClientId)).ReturnsAsync(valuation);
        _methodsRepo.Setup(x => x.GetByValuationAsync(valuation.Id, ClientId))
            .ReturnsAsync(Enumerable.Empty<ValuationMethodEntity>());

        var result = await _service.GetByIdAsync(valuation.Id, ClientId);

        result.Should().NotBeNull();
        result.Id.Should().Be(valuation.Id);
        result.Status.Should().Be("draft");
    }

    [Fact]
    public async Task GetByIdAsync_NotFound_ThrowsNotFoundException()
    {
        var id = Guid.NewGuid();
        _valuationsRepo.Setup(x => x.GetByIdAsync(id, ClientId)).ReturnsAsync((ValuationEntity?)null);

        var act = async () => await _service.GetByIdAsync(id, ClientId);
        await act.Should().ThrowAsync<NotFoundException>();
    }

    // ─── CreateAsync ─────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateAsync_ValidRequest_ReturnsCreatedValuation()
    {
        var request = new CreateValuationRequest
        {
            CompanyId = CompanyId,
            ValuationDate = DateTime.UtcNow,
            EventType = "seed",
            TotalShares = 10_000m
        };

        _valuationsRepo.Setup(x => x.AddAsync(It.IsAny<ValuationEntity>())).Returns(Task.CompletedTask);

        var result = await _service.CreateAsync(ClientId, request, UserId);

        result.Should().NotBeNull();
        result.Status.Should().Be("draft");
        result.CompanyId.Should().Be(CompanyId);
        _valuationsRepo.Verify(x => x.AddAsync(It.IsAny<ValuationEntity>()), Times.Once);
    }

    // ─── SubmitAsync ─────────────────────────────────────────────────────────

    [Fact]
    public async Task SubmitAsync_WithSelectedCalculatedMethod_ChangesToPendingApproval()
    {
        var valuation = MakeValuation();
        var method = MakeMethod(valuation.Id, calculatedValue: 1_000_000m, isSelected: true);

        _valuationsRepo.Setup(x => x.GetByIdAsync(valuation.Id, ClientId)).ReturnsAsync(valuation);
        _valuationsRepo.Setup(x => x.UpdateAsync(It.IsAny<ValuationEntity>())).Returns(Task.CompletedTask);
        _methodsRepo.Setup(x => x.GetSelectedAsync(valuation.Id, ClientId)).ReturnsAsync(method);
        _methodsRepo.Setup(x => x.GetByValuationAsync(valuation.Id, ClientId))
            .ReturnsAsync(new[] { method });

        var result = await _service.SubmitAsync(valuation.Id, ClientId, UserId);

        result.Status.Should().Be("pending_approval");
    }

    [Fact]
    public async Task SubmitAsync_WithNoSelectedMethod_ThrowsDomainException()
    {
        var valuation = MakeValuation();
        _valuationsRepo.Setup(x => x.GetByIdAsync(valuation.Id, ClientId)).ReturnsAsync(valuation);
        _methodsRepo.Setup(x => x.GetSelectedAsync(valuation.Id, ClientId))
            .ReturnsAsync((ValuationMethodEntity?)null);

        var act = async () => await _service.SubmitAsync(valuation.Id, ClientId, UserId);
        await act.Should().ThrowAsync<DomainException>();
    }

    // ─── ApproveAsync ─────────────────────────────────────────────────────────

    [Fact]
    public async Task ApproveAsync_PendingApproval_ComputesPricePerShare()
    {
        var valuation = MakeValuation();
        valuation.SetValuationAmount(1_000_000m, UserId);
        valuation.Submit(UserId);

        _valuationsRepo.Setup(x => x.GetByIdAsync(valuation.Id, ClientId)).ReturnsAsync(valuation);
        _valuationsRepo.Setup(x => x.UpdateAsync(It.IsAny<ValuationEntity>())).Returns(Task.CompletedTask);
        _methodsRepo.Setup(x => x.GetByValuationAsync(valuation.Id, ClientId))
            .ReturnsAsync(Enumerable.Empty<ValuationMethodEntity>());

        var result = await _service.ApproveAsync(valuation.Id, ClientId, UserId);

        result.Status.Should().Be("approved");
        result.PricePerShare.Should().BeApproximately(100m, 0.001m); // 1_000_000 / 10_000
    }

    // ─── RejectAsync ─────────────────────────────────────────────────────────

    [Fact]
    public async Task RejectAsync_PendingApproval_ChangesToRejected()
    {
        var valuation = MakeValuation();
        valuation.SetValuationAmount(1_000_000m, UserId);
        valuation.Submit(UserId);

        _valuationsRepo.Setup(x => x.GetByIdAsync(valuation.Id, ClientId)).ReturnsAsync(valuation);
        _valuationsRepo.Setup(x => x.UpdateAsync(It.IsAny<ValuationEntity>())).Returns(Task.CompletedTask);
        _methodsRepo.Setup(x => x.GetByValuationAsync(valuation.Id, ClientId))
            .ReturnsAsync(Enumerable.Empty<ValuationMethodEntity>());

        var result = await _service.RejectAsync(valuation.Id, ClientId, "Dados insuficientes", UserId);

        result.Status.Should().Be("rejected");
        result.RejectionReason.Should().Be("Dados insuficientes");
    }

    // ─── DeleteAsync ─────────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteAsync_ExistingId_SoftDeletes()
    {
        var valuation = MakeValuation();
        _valuationsRepo.Setup(x => x.GetByIdAsync(valuation.Id, ClientId)).ReturnsAsync(valuation);
        _valuationsRepo.Setup(x => x.SoftDeleteAsync(valuation.Id, ClientId, UserId)).Returns(Task.CompletedTask);

        await _service.DeleteAsync(valuation.Id, ClientId, UserId);

        _valuationsRepo.Verify(x => x.SoftDeleteAsync(valuation.Id, ClientId, UserId), Times.Once);
    }

    // ─── Helpers ─────────────────────────────────────────────────────────────

    private static ValuationEntity MakeValuation() =>
        ValuationEntity.Create(ClientId, CompanyId, DateTime.UtcNow, "seed", 10_000m, createdBy: UserId);

    private static ValuationMethodEntity MakeMethod(Guid valuationId, decimal? calculatedValue = null, bool isSelected = false)
    {
        var m = ValuationMethodEntity.Create(ClientId, valuationId, "arr_multiple", "{\"arr\":1000000,\"multiple\":5}", createdBy: UserId);
        if (calculatedValue.HasValue)
            m.SetCalculatedValue(calculatedValue.Value, UserId);
        if (isSelected)
            m.Select(UserId);
        return m;
    }
}
