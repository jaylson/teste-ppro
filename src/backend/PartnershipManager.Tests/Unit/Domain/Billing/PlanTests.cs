using PartnershipManager.Domain.Entities.Billing;

namespace PartnershipManager.Tests.Unit.Domain.Billing;

public class PlanTests
{
    [Fact]
    public void Plan_ShouldInitializeWithDefaultValues()
    {
        // Arrange & Act
        var plan = new Plan
        {
            Name = "Starter",
            Description = "Plano inicial",
            Price = 99.00m,
            BillingCycle = BillingCycle.Monthly,
            MaxCompanies = 1,
            MaxUsers = 5,
            IsActive = true
        };

        // Assert
        plan.Id.Should().NotBeEmpty();
        plan.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        plan.IsDeleted.Should().BeFalse();
        plan.Subscriptions.Should().NotBeNull();
    }

    [Theory]
    [InlineData(BillingCycle.Monthly)]
    [InlineData(BillingCycle.Yearly)]
    public void Plan_ShouldAcceptValidBillingCycles(BillingCycle cycle)
    {
        // Arrange & Act
        var plan = new Plan
        {
            Name = "Test Plan",
            Description = "Test",
            Price = 99.00m,
            BillingCycle = cycle,
            MaxCompanies = 1,
            MaxUsers = 5,
            IsActive = true
        };

        // Assert
        plan.BillingCycle.Should().Be(cycle);
    }

    [Theory]
    [InlineData(1, 5)]
    [InlineData(3, 20)]
    [InlineData(-1, -1)] // Ilimitado
    public void Plan_ShouldAcceptValidLimits(int maxCompanies, int maxUsers)
    {
        // Arrange & Act
        var plan = new Plan
        {
            Name = "Test Plan",
            Description = "Test",
            Price = 99.00m,
            BillingCycle = BillingCycle.Monthly,
            MaxCompanies = maxCompanies,
            MaxUsers = maxUsers,
            IsActive = true
        };

        // Assert
        plan.MaxCompanies.Should().Be(maxCompanies);
        plan.MaxUsers.Should().Be(maxUsers);
    }

    [Theory]
    [InlineData(99.00)]
    [InlineData(299.00)]
    [InlineData(999.00)]
    public void Plan_ShouldAcceptValidPrices(decimal price)
    {
        // Arrange & Act
        var plan = new Plan
        {
            Name = "Test Plan",
            Description = "Test",
            Price = price,
            BillingCycle = BillingCycle.Monthly,
            MaxCompanies = 1,
            MaxUsers = 5,
            IsActive = true
        };

        // Assert
        plan.Price.Should().Be(price);
    }

    [Fact]
    public void Plan_ShouldToggleActiveStatus()
    {
        // Arrange
        var plan = new Plan
        {
            Name = "Test Plan",
            Description = "Test",
            Price = 99.00m,
            BillingCycle = BillingCycle.Monthly,
            MaxCompanies = 1,
            MaxUsers = 5,
            IsActive = true
        };

        // Act
        plan.IsActive = false;

        // Assert
        plan.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Plan_ShouldStoreFeaturesAsJson()
    {
        // Arrange & Act
        var plan = new Plan
        {
            Name = "Professional",
            Description = "Plano profissional",
            Price = 299.00m,
            BillingCycle = BillingCycle.Monthly,
            MaxCompanies = 3,
            MaxUsers = 20,
            IsActive = true,
            Features = "[\"Cap Table\", \"Contratos\", \"Vesting\"]"
        };

        // Assert
        plan.Features.Should().NotBeNullOrEmpty();
        plan.Features.Should().Contain("Cap Table");
    }
}
