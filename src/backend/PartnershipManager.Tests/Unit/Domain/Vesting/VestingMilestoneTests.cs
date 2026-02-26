using PartnershipManager.Domain.Entities;
using PartnershipManager.Domain.Enums;

namespace PartnershipManager.Tests.Unit.Domain.Vesting;

public class VestingMilestoneTests
{
    private static VestingMilestone CreatePendingMilestone()
    {
        return VestingMilestone.Create(
            clientId: Guid.NewGuid(),
            vestingPlanId: Guid.NewGuid(),
            companyId: Guid.NewGuid(),
            name: "Primeiro Marco",
            description: "Marco de teste",
            milestoneType: MilestoneType.Financial,
            targetValue: 1_000_000m,
            targetUnit: "BRL",
            accelerationPercentage: 10m,
            isRequiredForFullVesting: false,
            targetDate: DateTime.UtcNow.AddMonths(6),
            createdBy: Guid.NewGuid());
    }

    // ──────────────────────────────────────────────
    // Create
    // ──────────────────────────────────────────────

    [Fact]
    public void Create_ShouldInitializeWithPendingStatus()
    {
        var milestone = CreatePendingMilestone();

        milestone.Status.Should().Be(MilestoneStatus.Pending);
        milestone.AchievedDate.Should().BeNull();
        milestone.AchievedBy.Should().BeNull();
    }

    [Fact]
    public void Create_ShouldSetPropertiesCorrectly()
    {
        var clientId = Guid.NewGuid();
        var planId = Guid.NewGuid();
        var companyId = Guid.NewGuid();

        var milestone = VestingMilestone.Create(
            clientId, planId, companyId,
            "Marco de Receita", MilestoneType.Financial, 5m, "desc",
            500_000m, "BRL", true, DateTime.UtcNow.AddMonths(3), Guid.NewGuid());

        milestone.ClientId.Should().Be(clientId);
        milestone.VestingPlanId.Should().Be(planId);
        milestone.CompanyId.Should().Be(companyId);
        milestone.Name.Should().Be("Marco de Receita");
        milestone.TargetValue.Should().Be(500_000m);
        milestone.AccelerationPercentage.Should().Be(5m);
        milestone.IsRequiredForFullVesting.Should().BeTrue();
    }

    // ──────────────────────────────────────────────
    // MarkAsAchieved
    // ──────────────────────────────────────────────

    [Fact]
    public void MarkAsAchieved_ShouldChangeStatusToAchieved()
    {
        var milestone = CreatePendingMilestone();
        var userId = Guid.NewGuid();
        var achievedDate = DateTime.UtcNow;

        milestone.MarkAsAchieved(userId, achievedDate, achievedValue: 1_200_000m);

        milestone.Status.Should().Be(MilestoneStatus.Achieved);
        milestone.AchievedDate.Should().Be(achievedDate.Date);
        milestone.AchievedBy.Should().Be(userId);
        milestone.AchievedValue.Should().Be(1_200_000m);
    }

    [Fact]
    public void MarkAsAchieved_ShouldThrow_IfAlreadyAchieved()
    {
        var milestone = CreatePendingMilestone();
        var userId = Guid.NewGuid();
        milestone.MarkAsAchieved(userId, DateTime.UtcNow);

        var act = () => milestone.MarkAsAchieved(userId, DateTime.UtcNow);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void MarkAsAchieved_ShouldAcceptNullAchievedValue()
    {
        var milestone = CreatePendingMilestone();
        milestone.MarkAsAchieved(Guid.NewGuid(), DateTime.UtcNow, achievedValue: null);

        milestone.AchievedValue.Should().BeNull();
        milestone.Status.Should().Be(MilestoneStatus.Achieved);
    }

    // ──────────────────────────────────────────────
    // MarkAsFailed
    // ──────────────────────────────────────────────

    [Fact]
    public void MarkAsFailed_ShouldChangeStatusToFailed()
    {
        var milestone = CreatePendingMilestone();

        milestone.MarkAsFailed(Guid.NewGuid());

        milestone.Status.Should().Be(MilestoneStatus.Failed);
    }

    [Fact]
    public void MarkAsFailed_ShouldThrow_IfAlreadyAchieved()
    {
        var milestone = CreatePendingMilestone();
        milestone.MarkAsAchieved(Guid.NewGuid(), DateTime.UtcNow);

        var act = () => milestone.MarkAsFailed(Guid.NewGuid());

        act.Should().Throw<InvalidOperationException>();
    }

    // ──────────────────────────────────────────────
    // Cancel
    // ──────────────────────────────────────────────

    [Fact]
    public void Cancel_ShouldChangeStatusToCancelled()
    {
        var milestone = CreatePendingMilestone();

        milestone.Cancel(Guid.NewGuid());

        milestone.Status.Should().Be(MilestoneStatus.Cancelled);
    }

    [Fact]
    public void Cancel_ShouldThrow_IfAlreadyAchieved()
    {
        var milestone = CreatePendingMilestone();
        milestone.MarkAsAchieved(Guid.NewGuid(), DateTime.UtcNow);

        var act = () => milestone.Cancel(Guid.NewGuid());

        act.Should().Throw<InvalidOperationException>();
    }
}
