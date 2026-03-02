using PartnershipManager.Domain.Entities;

namespace PartnershipManager.Tests.Unit.Domain.Financial;

public class FinancialPeriodTests
{
    private static FinancialPeriod CreatePeriod(short year = 2024, byte month = 1)
    {
        return FinancialPeriod.Create(
            clientId: Guid.NewGuid(),
            companyId: Guid.NewGuid(),
            year: year,
            month: month,
            notes: null,
            createdBy: Guid.NewGuid());
    }

    // ──────────────────────────────────────────────
    // Create
    // ──────────────────────────────────────────────

    [Fact]
    public void Create_ShouldInitializeWithDraftStatus()
    {
        var period = CreatePeriod();

        period.Status.Should().Be("draft");
        period.ApprovedAt.Should().BeNull();
        period.LockedAt.Should().BeNull();
    }

    // ──────────────────────────────────────────────
    // Workflow: draft -> submitted -> approved -> locked
    // ──────────────────────────────────────────────

    [Fact]
    public void Submit_WhenDraft_ShouldTransitionToSubmitted()
    {
        var period = CreatePeriod();

        period.Submit(Guid.NewGuid());

        period.Status.Should().Be("submitted");
    }

    [Fact]
    public void Approve_WhenSubmitted_ShouldTransitionToApproved()
    {
        var period = CreatePeriod();
        period.Submit(Guid.NewGuid());
        var approver = Guid.NewGuid();

        period.Approve(approver);

        period.Status.Should().Be("approved");
        period.ApprovedAt.Should().NotBeNull();
        period.ApprovedBy.Should().Be(approver);
    }

    [Fact]
    public void Lock_WhenApproved_ShouldTransitionToLocked()
    {
        var period = CreatePeriod();
        period.Submit(Guid.NewGuid());
        period.Approve(Guid.NewGuid());

        period.Lock(Guid.NewGuid());

        period.Status.Should().Be("locked");
        period.LockedAt.Should().NotBeNull();
    }

    [Fact]
    public void Approve_WhenDraft_ShouldThrow()
    {
        var period = CreatePeriod();

        var act = () => period.Approve(Guid.NewGuid());

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Lock_WhenSubmitted_ShouldThrow()
    {
        var period = CreatePeriod();
        period.Submit(Guid.NewGuid());

        var act = () => period.Lock(Guid.NewGuid());

        act.Should().Throw<InvalidOperationException>();
    }

    // ──────────────────────────────────────────────
    // FI-03 — locked period cannot be edited
    // ──────────────────────────────────────────────

    [Fact]
    public void CanBeEdited_WhenLocked_ShouldBeFalse()
    {
        var period = CreatePeriod();
        period.Submit(Guid.NewGuid());
        period.Approve(Guid.NewGuid());
        period.Lock(Guid.NewGuid());

        period.CanBeEdited.Should().BeFalse();
    }

    [Fact]
    public void CanBeEdited_WhenDraft_ShouldBeTrue()
    {
        var period = CreatePeriod();

        period.CanBeEdited.Should().BeTrue();
    }

    [Fact]
    public void CanBeEdited_WhenSubmitted_ShouldBeTrue()
    {
        var period = CreatePeriod();
        period.Submit(Guid.NewGuid());

        period.CanBeEdited.Should().BeTrue();
    }

    // ──────────────────────────────────────────────
    // ReturnToSubmitted
    // ──────────────────────────────────────────────

    [Fact]
    public void ReturnToSubmitted_WhenApproved_ShouldAllowReApproval()
    {
        var period = CreatePeriod();
        period.Submit(Guid.NewGuid());
        period.Approve(Guid.NewGuid());

        period.ReturnToSubmitted(Guid.NewGuid());

        period.Status.Should().Be("submitted");
        period.ApprovedAt.Should().BeNull();
    }

    // ──────────────────────────────────────────────
    // Month / Year validation
    // ──────────────────────────────────────────────

    [Theory]
    [InlineData(2024, (byte)0)]
    [InlineData(2024, (byte)13)]
    public void Create_WithInvalidMonth_ShouldThrow(short year, byte month)
    {
        var act = () => FinancialPeriod.Create(
            Guid.NewGuid(), Guid.NewGuid(), year, month, null, Guid.NewGuid());

        act.Should().Throw<ArgumentOutOfRangeException>();
    }
}
