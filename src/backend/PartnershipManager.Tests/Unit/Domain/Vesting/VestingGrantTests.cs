using PartnershipManager.Domain.Entities;
using PartnershipManager.Domain.Enums;

namespace PartnershipManager.Tests.Unit.Domain.Vesting;

public class VestingGrantTests
{
    private static VestingGrant CreateActiveGrant(
        DateTime grantDate,
        int cliffMonths = 12,
        int vestingMonths = 48,
        decimal totalShares = 1000m)
    {
        var grant = VestingGrant.Create(
            clientId: Guid.NewGuid(),
            vestingPlanId: Guid.NewGuid(),
            shareholderId: Guid.NewGuid(),
            companyId: Guid.NewGuid(),
            grantDate: grantDate,
            totalShares: totalShares,
            sharePrice: 10m,
            equityPercentage: 2m,
            vestingStartDate: grantDate,
            vestingMonths: vestingMonths,
            cliffMonths: cliffMonths,
            notes: null,
            createdBy: Guid.NewGuid());

        grant.Approve(Guid.NewGuid());
        grant.Activate(Guid.NewGuid());
        return grant;
    }

    // ──────────────────────────────────────────────
    // Create
    // ──────────────────────────────────────────────

    [Fact]
    public void Create_ShouldInitializeWithPendingStatus()
    {
        var grant = VestingGrant.Create(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            DateTime.UtcNow, 1000, 10m, 2m,
            DateTime.UtcNow, 48, 12, null, Guid.NewGuid());

        grant.Status.Should().Be(VestingGrantDetailStatus.Pending);
        grant.VestedShares.Should().Be(0);
        grant.ExercisedShares.Should().Be(0);
    }

    // ──────────────────────────────────────────────
    // Cliff
    // ──────────────────────────────────────────────

    [Fact]
    public void IsCliffMet_ShouldReturnFalse_BeforeCliffDate()
    {
        var grantDate = DateTime.UtcNow.AddMonths(-6); // 6 meses atrás
        var grant = CreateActiveGrant(grantDate, cliffMonths: 12);

        grant.IsCliffMet(DateTime.UtcNow).Should().BeFalse();
    }

    [Fact]
    public void IsCliffMet_ShouldReturnTrue_AfterCliffDate()
    {
        var grantDate = DateTime.UtcNow.AddMonths(-18); // 18 meses atrás
        var grant = CreateActiveGrant(grantDate, cliffMonths: 12);

        grant.IsCliffMet(DateTime.UtcNow).Should().BeTrue();
    }

    [Fact]
    public void IsCliffMet_ShouldReturnTrue_WhenNoCliff()
    {
        var grant = CreateActiveGrant(DateTime.UtcNow.AddMonths(-1), cliffMonths: 0);

        grant.IsCliffMet(DateTime.UtcNow).Should().BeTrue();
    }

    // ──────────────────────────────────────────────
    // CalculateVestedShares
    // ──────────────────────────────────────────────

    [Fact]
    public void CalculateVestedShares_ShouldReturnZero_BeforeCliff()
    {
        var grantDate = DateTime.UtcNow.AddMonths(-6);
        var grant = CreateActiveGrant(grantDate, cliffMonths: 12, vestingMonths: 48, totalShares: 1200);

        var vested = grant.CalculateVestedShares(DateTime.UtcNow);

        vested.Should().Be(0);
    }

    [Fact]
    public void CalculateVestedShares_ShouldReturnProportional_AfterCliff()
    {
        // Grant de 1200 ações, vesting 12 meses (mensal), sem cliff (cliffMonths=0)
        // Após 6 meses → 50% = 600 ações
        var grantDate = DateTime.UtcNow.AddMonths(-6);
        var grant = CreateActiveGrant(grantDate, cliffMonths: 0, vestingMonths: 12, totalShares: 1200);

        var vested = grant.CalculateVestedShares(DateTime.UtcNow);

        // Should be roughly 600, allowing one period of tolerance
        vested.Should().BeGreaterThanOrEqualTo(400).And.BeLessThanOrEqualTo(700);
    }

    [Fact]
    public void CalculateVestedShares_ShouldReturnTotal_AfterFullVesting()
    {
        var grantDate = DateTime.UtcNow.AddYears(-5); // 5 anos atrás, vesting de 4 anos
        var grant = CreateActiveGrant(grantDate, cliffMonths: 12, vestingMonths: 48, totalShares: 1000);

        var vested = grant.CalculateVestedShares(DateTime.UtcNow);

        vested.Should().Be(1000);
    }

    // ──────────────────────────────────────────────
    // ExerciseShares
    // ──────────────────────────────────────────────

    [Fact]
    public void ExerciseShares_ShouldReduceAvailableToExercise()
    {
        var grantDate = DateTime.UtcNow.AddYears(-5);
        var grant = CreateActiveGrant(grantDate, cliffMonths: 12, vestingMonths: 48, totalShares: 1000);
        grant.RecalculateVestedShares(DateTime.UtcNow, Guid.NewGuid());

        grant.ExerciseShares(200, Guid.NewGuid());

        grant.ExercisedShares.Should().Be(200);
        grant.AvailableToExercise.Should().Be(800);
    }

    [Fact]
    public void ExerciseShares_ShouldThrow_WhenExceedsVested()
    {
        var grantDate = DateTime.UtcNow.AddMonths(-18); // dentro do vesting, não totalmente vestido
        var grant = CreateActiveGrant(grantDate, cliffMonths: 12, vestingMonths: 48, totalShares: 1000);
        grant.RecalculateVestedShares(DateTime.UtcNow, Guid.NewGuid());

        // Tentar exercitar mais do que está disponível
        var act = () => grant.ExerciseShares(grant.AvailableToExercise + 1, Guid.NewGuid());

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void ExerciseShares_ShouldThrow_WhenZeroOrNegative()
    {
        var grantDate = DateTime.UtcNow.AddYears(-5);
        var grant = CreateActiveGrant(grantDate, cliffMonths: 0, vestingMonths: 12, totalShares: 1000);
        grant.RecalculateVestedShares(DateTime.UtcNow, Guid.NewGuid());

        var act = () => grant.ExerciseShares(0, Guid.NewGuid());

        act.Should().Throw<ArgumentException>();
    }

    // ──────────────────────────────────────────────
    // Approve / Activate / Cancel lifecycle
    // ──────────────────────────────────────────────

    [Fact]
    public void Approve_ShouldSetApprovalData()
    {
        var grant = VestingGrant.Create(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            DateTime.UtcNow, 1000, 10m, 2m,
            DateTime.UtcNow, 48, 12, null, Guid.NewGuid());

        var approver = Guid.NewGuid();
        grant.Approve(approver);

        grant.Status.Should().Be(VestingGrantDetailStatus.Approved);
        grant.ApprovedBy.Should().Be(approver);
        grant.ApprovedAt.Should().NotBeNull();
    }

    [Fact]
    public void Cancel_ShouldSetCancelledStatus()
    {
        var grant = VestingGrant.Create(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            DateTime.UtcNow, 1000, 10m, 2m,
            DateTime.UtcNow, 48, 12, null, Guid.NewGuid());

        grant.Cancel(Guid.NewGuid());

        grant.Status.Should().Be(VestingGrantDetailStatus.Cancelled);
    }

    // ──────────────────────────────────────────────
    // Computed properties
    // ──────────────────────────────────────────────

    [Fact]
    public void IsFullyVested_ShouldReturnTrue_WhenAllSharesVested()
    {
        var grantDate = DateTime.UtcNow.AddYears(-5);
        var grant = CreateActiveGrant(grantDate, cliffMonths: 12, vestingMonths: 48, totalShares: 500);
        grant.RecalculateVestedShares(DateTime.UtcNow, Guid.NewGuid());

        grant.IsFullyVested.Should().BeTrue();
    }

    [Fact]
    public void IsFullyExercised_ShouldReturnFalse_Initially()
    {
        var grantDate = DateTime.UtcNow.AddYears(-5);
        var grant = CreateActiveGrant(grantDate, cliffMonths: 0, vestingMonths: 12, totalShares: 100);

        grant.IsFullyExercised.Should().BeFalse();
    }

    [Fact]
    public void GetFutureProjection_ShouldReturnPositiveVesting_ForFutureDate()
    {
        var grantDate = DateTime.UtcNow;
        var grant = CreateActiveGrant(grantDate, cliffMonths: 0, vestingMonths: 24, totalShares: 2400);

        var (vestedShares, vestedPct) = grant.GetFutureProjection(DateTime.UtcNow.AddMonths(12));

        vestedShares.Should().BeGreaterThan(0);
        vestedPct.Should().BeGreaterThan(0);
    }
}
