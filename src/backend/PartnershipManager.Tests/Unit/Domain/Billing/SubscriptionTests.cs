using PartnershipManager.Domain.Entities.Billing;

namespace PartnershipManager.Tests.Unit.Domain.Billing;

public class SubscriptionTests
{
    [Fact]
    public void Subscription_ShouldInitializeWithDefaultValues()
    {
        // Arrange & Act
        var subscription = new Subscription
        {
            ClientId = Guid.NewGuid(),
            PlanId = Guid.NewGuid(),
            StartDate = DateTime.UtcNow,
            AutoRenew = true,
            Status = SubscriptionStatus.Pending
        };

        // Assert
        subscription.Id.Should().NotBeEmpty();
        subscription.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        subscription.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        subscription.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public void Activate_ShouldChangeStatusToActive()
    {
        // Arrange
        var subscription = new Subscription
        {
            ClientId = Guid.NewGuid(),
            PlanId = Guid.NewGuid(),
            StartDate = DateTime.UtcNow,
            Status = SubscriptionStatus.Pending
        };
        var beforeUpdate = subscription.UpdatedAt;

        // Act
        System.Threading.Thread.Sleep(10); // Garantir diferença no timestamp
        subscription.Activate();

        // Assert
        subscription.Status.Should().Be(SubscriptionStatus.Active);
        subscription.UpdatedAt.Should().BeAfter(beforeUpdate);
    }

    [Fact]
    public void Suspend_ShouldChangeStatusToSuspended()
    {
        // Arrange
        var subscription = new Subscription
        {
            ClientId = Guid.NewGuid(),
            PlanId = Guid.NewGuid(),
            StartDate = DateTime.UtcNow,
            Status = SubscriptionStatus.Active
        };
        var beforeUpdate = subscription.UpdatedAt;

        // Act
        System.Threading.Thread.Sleep(10);
        subscription.Suspend();

        // Assert
        subscription.Status.Should().Be(SubscriptionStatus.Suspended);
        subscription.UpdatedAt.Should().BeAfter(beforeUpdate);
    }

    [Fact]
    public void Cancel_ShouldChangeStatusToCancelledAndDisableAutoRenew()
    {
        // Arrange
        var subscription = new Subscription
        {
            ClientId = Guid.NewGuid(),
            PlanId = Guid.NewGuid(),
            StartDate = DateTime.UtcNow,
            Status = SubscriptionStatus.Active,
            AutoRenew = true
        };
        var beforeUpdate = subscription.UpdatedAt;

        // Act
        System.Threading.Thread.Sleep(10);
        subscription.Cancel();

        // Assert
        subscription.Status.Should().Be(SubscriptionStatus.Cancelled);
        subscription.AutoRenew.Should().BeFalse();
        subscription.EndDate.Should().NotBeNull();
        subscription.EndDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        subscription.UpdatedAt.Should().BeAfter(beforeUpdate);
    }

    [Fact]
    public void Subscription_ShouldTrackUsage()
    {
        // Arrange
        var subscription = new Subscription
        {
            ClientId = Guid.NewGuid(),
            PlanId = Guid.NewGuid(),
            StartDate = DateTime.UtcNow,
            CompaniesCount = 0,
            UsersCount = 0
        };

        // Act
        subscription.CompaniesCount = 2;
        subscription.UsersCount = 15;

        // Assert
        subscription.CompaniesCount.Should().Be(2);
        subscription.UsersCount.Should().Be(15);
    }
}
