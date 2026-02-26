using PartnershipManager.Domain.Entities;
using PartnershipManager.Domain.Enums;

namespace PartnershipManager.Tests.Unit.Domain.Vesting;

public class VestingPlanTests
{
    private static VestingPlan CreateDraftPlan()
    {
        return VestingPlan.Create(
            clientId: Guid.NewGuid(),
            companyId: Guid.NewGuid(),
            name: "Plano 4 anos",
            vestingType: VestingType.TimeBasedLinear,
            cliffMonths: 12,
            vestingMonths: 48,
            totalEquityPercentage: 5.0m,
            description: "Cliff de 1 ano + 3 anos de vesting",
            createdBy: Guid.NewGuid());
    }

    // ──────────────────────────────────────────────
    // Create
    // ──────────────────────────────────────────────

    [Fact]
    public void Create_ShouldReturnDraftStatus()
    {
        var plan = CreateDraftPlan();

        plan.Status.Should().Be(VestingPlanStatus.Draft);
    }

    [Fact]
    public void Create_ShouldSetPropertiesCorrectly()
    {
        var clientId = Guid.NewGuid();
        var companyId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var plan = VestingPlan.Create(
            clientId, companyId, "Plan X", VestingType.HybridTimeMilestone, 6, 24, 10m, "desc", userId);

        plan.ClientId.Should().Be(clientId);
        plan.CompanyId.Should().Be(companyId);
        plan.Name.Should().Be("Plan X");
        plan.CliffMonths.Should().Be(6);
        plan.VestingMonths.Should().Be(24);
        plan.TotalEquityPercentage.Should().Be(10m);
        plan.VestingType.Should().Be(VestingType.HybridTimeMilestone);
        plan.Id.Should().NotBeEmpty();
    }

    // ──────────────────────────────────────────────
    // Activate
    // ──────────────────────────────────────────────

    [Fact]
    public void Activate_ShouldChangeStatusToActive_WhenDraft()
    {
        var plan = CreateDraftPlan();
        var userId = Guid.NewGuid();

        plan.Activate(userId);

        plan.Status.Should().Be(VestingPlanStatus.Active);
        plan.ActivatedAt.Should().NotBeNull();
        plan.ActivatedBy.Should().Be(userId);
    }

    [Fact]
    public void Activate_ShouldThrow_WhenAlreadyActive()
    {
        var plan = CreateDraftPlan();
        var userId = Guid.NewGuid();
        plan.Activate(userId);

        var act = () => plan.Activate(userId);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void CanBeActivated_ShouldBeFalse_WhenAlreadyActive()
    {
        var plan = CreateDraftPlan();
        plan.Activate(Guid.NewGuid());

        plan.CanBeActivated().Should().BeFalse();
    }

    // ──────────────────────────────────────────────
    // Deactivate
    // ──────────────────────────────────────────────

    [Fact]
    public void Deactivate_ShouldChangeToDraft_WhenActive()
    {
        var plan = CreateDraftPlan();
        plan.Activate(Guid.NewGuid());

        plan.Deactivate(Guid.NewGuid());

        plan.Status.Should().Be(VestingPlanStatus.Inactive);
    }

    [Fact]
    public void Deactivate_ShouldThrow_WhenNotActive()
    {
        var plan = CreateDraftPlan();

        var act = () => plan.Deactivate(Guid.NewGuid());

        act.Should().Throw<InvalidOperationException>();
    }

    // ──────────────────────────────────────────────
    // Archive
    // ──────────────────────────────────────────────

    [Fact]
    public void Archive_ShouldChangeStatusToArchived()
    {
        var plan = CreateDraftPlan();
        plan.Activate(Guid.NewGuid());

        plan.Archive(Guid.NewGuid());

        plan.Status.Should().Be(VestingPlanStatus.Archived);
    }

    [Fact]
    public void Archive_ShouldThrow_WhenAlreadyArchived()
    {
        var plan = CreateDraftPlan();
        plan.Activate(Guid.NewGuid());
        plan.Archive(Guid.NewGuid());

        var act = () => plan.Archive(Guid.NewGuid());

        act.Should().Throw<InvalidOperationException>();
    }

    // ──────────────────────────────────────────────
    // UpdateDetails
    // ──────────────────────────────────────────────

    [Fact]
    public void UpdateDetails_ShouldChangeNameAndDescription()
    {
        var plan = CreateDraftPlan();

        plan.UpdateDetails("Novo Nome", "Nova Descricao", 6, 24, 5.0m, Guid.NewGuid());

        plan.Name.Should().Be("Novo Nome");
        plan.Description.Should().Be("Nova Descricao");
    }

    // ──────────────────────────────────────────────
    // IsActive helper
    // ──────────────────────────────────────────────

    [Fact]
    public void IsActive_ShouldReturnFalse_WhenDraft()
    {
        var plan = CreateDraftPlan();
        plan.IsActive().Should().BeFalse();
    }

    [Fact]
    public void IsActive_ShouldReturnTrue_WhenActive()
    {
        var plan = CreateDraftPlan();
        plan.Activate(Guid.NewGuid());
        plan.IsActive().Should().BeTrue();
    }
}
