using PartnershipManager.Application.Features.Financial.DTOs;
using PartnershipManager.Domain.Exceptions;
using PartnershipManager.Infrastructure.Services.Financial;

namespace PartnershipManager.Tests.Unit.Application.Services;

public class FinancialPeriodServiceTests
{
    private readonly Mock<IUnitOfWork> _uow;
    private readonly Mock<IFinancialPeriodRepository> _periodsRepo;
    private readonly Mock<IFinancialMetricRepository> _metricsRepo;
    private readonly FinancialPeriodService _service;

    private static readonly Guid ClientId = Guid.NewGuid();
    private static readonly Guid CompanyId = Guid.NewGuid();
    private static readonly Guid UserId = Guid.NewGuid();

    public FinancialPeriodServiceTests()
    {
        _uow = new Mock<IUnitOfWork>();
        _periodsRepo = new Mock<IFinancialPeriodRepository>();
        _metricsRepo = new Mock<IFinancialMetricRepository>();

        _uow.SetupGet(x => x.FinancialPeriods).Returns(_periodsRepo.Object);
        _uow.SetupGet(x => x.FinancialMetrics).Returns(_metricsRepo.Object);

        _service = new FinancialPeriodService(_uow.Object);
    }

    // ─── CreateAsync ─────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateAsync_NewPeriod_ReturnsCreatedResponse()
    {
        var request = new CreateFinancialPeriodRequest
        {
            CompanyId = CompanyId,
            Year = 2025,
            Month = 1
        };

        _periodsRepo.Setup(x => x.ExistsAsync(ClientId, CompanyId, 2025, 1)).ReturnsAsync(false);
        _periodsRepo.Setup(x => x.AddAsync(It.IsAny<FinancialPeriod>())).Returns(Task.CompletedTask);

        var result = await _service.CreateAsync(ClientId, request, UserId);

        result.Should().NotBeNull();
        result.Year.Should().Be(2025);
        result.Month.Should().Be(1);
        result.Status.Should().Be("draft");
    }

    [Fact]
    public async Task CreateAsync_DuplicatePeriod_ThrowsDomainException()
    {
        var request = new CreateFinancialPeriodRequest
        {
            CompanyId = CompanyId,
            Year = 2025,
            Month = 1
        };
        _periodsRepo.Setup(x => x.ExistsAsync(ClientId, CompanyId, 2025, 1)).ReturnsAsync(true);

        var act = async () => await _service.CreateAsync(ClientId, request, UserId);
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*já existe*");
    }

    // ─── SubmitAsync ─────────────────────────────────────────────────────────

    [Fact]
    public async Task SubmitAsync_DraftPeriod_ChangesToSubmitted()
    {
        var period = MakePeriod();
        _periodsRepo.Setup(x => x.GetByIdAsync(period.Id, ClientId)).ReturnsAsync(period);
        _periodsRepo.Setup(x => x.UpdateAsync(It.IsAny<FinancialPeriod>())).Returns(Task.CompletedTask);
        _metricsRepo.Setup(x => x.GetByPeriodAsync(period.Id, ClientId)).ReturnsAsync((FinancialMetric?)null);

        var result = await _service.SubmitAsync(period.Id, ClientId, UserId);

        result.Status.Should().Be("submitted");
        result.SubmittedAt.Should().NotBeNull();
    }

    // ─── ApproveAsync (FI-02) ─────────────────────────────────────────────────

    [Fact]
    public async Task ApproveAsync_WithApprovedPreviousPeriod_Succeeds()
    {
        var period = MakePeriod(year: 2025, month: 2);
        period.Submit(UserId); // put it in submitted state

        var prevPeriod = MakePeriod(year: 2025, month: 1);
        prevPeriod.Submit(UserId);
        prevPeriod.Approve(UserId); // previous is approved

        _periodsRepo.Setup(x => x.GetByIdAsync(period.Id, ClientId)).ReturnsAsync(period);
        _periodsRepo.Setup(x => x.GetPreviousPeriodAsync(ClientId, CompanyId, 2025, 2)).ReturnsAsync(prevPeriod);
        _periodsRepo.Setup(x => x.UpdateAsync(It.IsAny<FinancialPeriod>())).Returns(Task.CompletedTask);
        _metricsRepo.Setup(x => x.GetByPeriodAsync(period.Id, ClientId)).ReturnsAsync((FinancialMetric?)null);

        var result = await _service.ApproveAsync(period.Id, ClientId, UserId);

        result.Status.Should().Be("approved");
    }

    [Fact]
    public async Task ApproveAsync_WithUnapprovedPreviousPeriod_ThrowsDomainException()
    {
        var period = MakePeriod(year: 2025, month: 2);
        period.Submit(UserId);

        var prevPeriod = MakePeriod(year: 2025, month: 1);
        // prevPeriod is draft (not approved, not locked)

        _periodsRepo.Setup(x => x.GetByIdAsync(period.Id, ClientId)).ReturnsAsync(period);
        _periodsRepo.Setup(x => x.GetPreviousPeriodAsync(ClientId, CompanyId, 2025, 2)).ReturnsAsync(prevPeriod);

        var act = async () => await _service.ApproveAsync(period.Id, ClientId, UserId);
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*FI-02*");
    }

    [Fact]
    public async Task ApproveAsync_NoGap_NoPreviousPeriod_Succeeds()
    {
        var period = MakePeriod(year: 2025, month: 1);
        period.Submit(UserId);

        _periodsRepo.Setup(x => x.GetByIdAsync(period.Id, ClientId)).ReturnsAsync(period);
        _periodsRepo.Setup(x => x.GetPreviousPeriodAsync(ClientId, CompanyId, 2025, 1))
            .ReturnsAsync((FinancialPeriod?)null); // no previous period
        _periodsRepo.Setup(x => x.UpdateAsync(It.IsAny<FinancialPeriod>())).Returns(Task.CompletedTask);
        _metricsRepo.Setup(x => x.GetByPeriodAsync(period.Id, ClientId)).ReturnsAsync((FinancialMetric?)null);

        var result = await _service.ApproveAsync(period.Id, ClientId, UserId);

        result.Status.Should().Be("approved");
    }

    // ─── UpsertRevenueAsync ──────────────────────────────────────────────────

    [Fact]
    public async Task UpsertRevenueAsync_NewMetrics_CreatesMetricRecord()
    {
        var period = MakePeriod();
        var request = new UpsertRevenueRequest { Mrr = 100_000m, GrossRevenue = 110_000m };

        _periodsRepo.Setup(x => x.GetByIdAsync(period.Id, ClientId)).ReturnsAsync(period);
        _metricsRepo.Setup(x => x.GetByPeriodAsync(period.Id, ClientId)).ReturnsAsync((FinancialMetric?)null);
        _metricsRepo.Setup(x => x.AddAsync(It.IsAny<FinancialMetric>())).Returns(Task.CompletedTask);

        var result = await _service.UpsertRevenueAsync(period.Id, ClientId, request, UserId);

        result.Should().NotBeNull();
        result.Mrr.Should().Be(100_000m);
        result.Arr.Should().Be(1_200_000m); // MRR × 12
    }

    // ─── LockAsync ───────────────────────────────────────────────────────────

    [Fact]
    public async Task LockAsync_ApprovedPeriod_ChangesToLocked()
    {
        var period = MakePeriod();
        period.Submit(UserId);
        period.Approve(UserId);

        _periodsRepo.Setup(x => x.GetByIdAsync(period.Id, ClientId)).ReturnsAsync(period);
        _periodsRepo.Setup(x => x.UpdateAsync(It.IsAny<FinancialPeriod>())).Returns(Task.CompletedTask);
        _metricsRepo.Setup(x => x.GetByPeriodAsync(period.Id, ClientId)).ReturnsAsync((FinancialMetric?)null);

        var result = await _service.LockAsync(period.Id, ClientId, UserId);

        result.Status.Should().Be("locked");
        result.LockedAt.Should().NotBeNull();
    }

    // ─── GetByIdAsync ─────────────────────────────────────────────────────────

    [Fact]
    public async Task GetByIdAsync_NotFound_ThrowsNotFoundException()
    {
        var id = Guid.NewGuid();
        _periodsRepo.Setup(x => x.GetByIdAsync(id, ClientId)).ReturnsAsync((FinancialPeriod?)null);

        var act = async () => await _service.GetByIdAsync(id, ClientId);
        await act.Should().ThrowAsync<NotFoundException>();
    }

    // ─── Helpers ─────────────────────────────────────────────────────────────

    private static FinancialPeriod MakePeriod(short year = 2025, byte month = 1) =>
        FinancialPeriod.Create(ClientId, CompanyId, year, month, createdBy: UserId);
}
