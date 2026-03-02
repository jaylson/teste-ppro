using PartnershipManager.Domain.Entities;
using ValuationEntity = PartnershipManager.Domain.Entities.Valuation;

namespace PartnershipManager.Tests.Unit.Domain.Valuation;

public class ValuationTests
{
    private static (Guid ClientId, Guid CompanyId, Guid UserId) Ids() =>
        (Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

    // helper: creates a valuation with amount set so Submit() works
    private static ValuationEntity CreateAndSetAmount(
        Guid clientId, Guid companyId, Guid userId,
        decimal totalShares = 10_000m, decimal amount = 1_000_000m)
    {
        var v = ValuationEntity.Create(
            clientId, companyId, DateTime.UtcNow, "seed", totalShares, createdBy: userId);
        v.SetValuationAmount(amount, userId);
        return v;
    }

    // ──────────────────────────────────────────────
    // Create
    // ──────────────────────────────────────────────

    [Fact]
    public void Create_ShouldInitializeWithDraftStatus()
    {
        var (clientId, companyId, userId) = Ids();
        var v = ValuationEntity.Create(
            clientId, companyId, DateTime.UtcNow, "seed", 10_000m, createdBy: userId);

        v.Status.Should().Be("draft");
        v.ValuationAmount.Should().BeNull();
        v.PricePerShare.Should().BeNull();
        v.ApprovedAt.Should().BeNull();
    }

    [Fact]
    public void Create_WithInvalidEventType_ShouldThrow()
    {
        var (clientId, companyId, userId) = Ids();
        var act = () => ValuationEntity.Create(
            clientId, companyId, DateTime.UtcNow, "invalid_type", 10_000m, createdBy: userId);

        act.Should().Throw<ArgumentException>();
    }

    // ──────────────────────────────────────────────
    // Submit
    // ──────────────────────────────────────────────

    [Fact]
    public void Submit_ShouldChangeToPendingApproval()
    {
        var (clientId, companyId, userId) = Ids();
        var v = CreateAndSetAmount(clientId, companyId, userId);

        v.Submit(userId);

        v.Status.Should().Be("pending_approval");
    }

    [Fact]
    public void Submit_WithoutAmount_ShouldThrow()
    {
        var (clientId, companyId, userId) = Ids();
        var v = ValuationEntity.Create(
            clientId, companyId, DateTime.UtcNow, "seed", 10_000m, createdBy: userId);
        // ValuationAmount is null

        var act = () => v.Submit(userId);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Submit_WhenAlreadyPending_ShouldThrow()
    {
        var (clientId, companyId, userId) = Ids();
        var v = CreateAndSetAmount(clientId, companyId, userId);
        v.Submit(userId);

        var act = () => v.Submit(userId);

        act.Should().Throw<InvalidOperationException>();
    }

    // ──────────────────────────────────────────────
    // Approve — VA-04 PricePerShare calculation
    // ──────────────────────────────────────────────

    [Fact]
    public void Approve_ShouldComputePricePerShare_VA04()
    {
        var (clientId, companyId, userId) = Ids();
        var v = CreateAndSetAmount(clientId, companyId, userId, totalShares: 10_000m, amount: 1_000_000m);
        v.Submit(userId);

        v.Approve(userId);

        v.Status.Should().Be("approved");
        v.ValuationAmount.Should().Be(1_000_000m);
        // VA-04: PricePerShare = ValuationAmount / TotalShares = 1_000_000 / 10_000 = 100
        v.PricePerShare.Should().Be(100m);
        v.ApprovedAt.Should().NotBeNull();
    }

    [Fact]
    public void Approve_WhenDraft_ShouldThrow()
    {
        var (clientId, companyId, userId) = Ids();
        var v = CreateAndSetAmount(clientId, companyId, userId);

        var act = () => v.Approve(userId);

        act.Should().Throw<InvalidOperationException>();
    }

    // ──────────────────────────────────────────────
    // Reject
    // ──────────────────────────────────────────────

    [Fact]
    public void Reject_ShouldChangeToRejectedStatus()
    {
        var (clientId, companyId, userId) = Ids();
        var v = CreateAndSetAmount(clientId, companyId, userId);
        v.Submit(userId);

        v.Reject(userId, "Dados incompletos");

        v.Status.Should().Be("rejected");
        v.RejectionReason.Should().Be("Dados incompletos");
    }

    [Fact]
    public void Reject_WhenDraft_ShouldThrow()
    {
        var (clientId, companyId, userId) = Ids();
        var v = CreateAndSetAmount(clientId, companyId, userId);

        var act = () => v.Reject(userId, "motivo");

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Reject_WithEmptyReason_ShouldThrow()
    {
        var (clientId, companyId, userId) = Ids();
        var v = CreateAndSetAmount(clientId, companyId, userId);
        v.Submit(userId);

        var act = () => v.Reject(userId, "");

        act.Should().Throw<ArgumentException>();
    }

    // ──────────────────────────────────────────────
    // ReturnToDraft
    // ──────────────────────────────────────────────

    [Fact]
    public void ReturnToDraft_WhenRejected_ShouldAllowResubmit()
    {
        var (clientId, companyId, userId) = Ids();
        var v = CreateAndSetAmount(clientId, companyId, userId);
        v.Submit(userId);
        v.Reject(userId, "Dados incompletos");

        v.ReturnToDraft(userId);
        v.Status.Should().Be("draft");

        // deve conseguir submeter novamente
        v.Submit(userId);
        v.Status.Should().Be("pending_approval");
    }

    [Fact]
    public void ReturnToDraft_WhenNotRejected_ShouldThrow()
    {
        var (clientId, companyId, userId) = Ids();
        var v = CreateAndSetAmount(clientId, companyId, userId);
        // status = draft

        var act = () => v.ReturnToDraft(userId);

        act.Should().Throw<InvalidOperationException>();
    }
}

public class ValuationMethodTests
{
    [Fact]
    public void Create_WithCustomType_RequiresFormulaVersionId()
    {
        var act = () => ValuationMethod.Create(
            Guid.NewGuid(), Guid.NewGuid(),
            "custom",
            formulaVersionId: null);  // obrigatório quando type=custom

        act.Should().Throw<ArgumentException>()
           .WithMessage("*FormulaVersionId*");
    }

    [Fact]
    public void Create_WithCustomType_AndFormulaVersionId_ShouldSucceed()
    {
        var method = ValuationMethod.Create(
            Guid.NewGuid(), Guid.NewGuid(),
            "custom",
            formulaVersionId: Guid.NewGuid());

        method.Should().NotBeNull();
        method.MethodType.Should().Be("custom");
    }

    [Fact]
    public void Select_ShouldMarkMethodAsSelected()
    {
        var method = ValuationMethod.Create(
            Guid.NewGuid(), Guid.NewGuid(), "dcf");

        method.Select(Guid.NewGuid());

        method.IsSelected.Should().BeTrue();
    }

    [Fact]
    public void Deselect_ShouldMarkMethodAsNotSelected()
    {
        var method = ValuationMethod.Create(
            Guid.NewGuid(), Guid.NewGuid(), "dcf");
        method.Select(Guid.NewGuid());

        method.Deselect(Guid.NewGuid());

        method.IsSelected.Should().BeFalse();
    }
}
