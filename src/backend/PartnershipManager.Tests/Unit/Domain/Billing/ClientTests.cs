using PartnershipManager.Domain.Entities.Billing;
using BillingClient = PartnershipManager.Domain.Entities.Billing.Client;

namespace PartnershipManager.Tests.Unit.Domain.Billing;

public class ClientTests
{
    [Fact]
    public void Client_ShouldInitializeWithDefaultValues()
    {
        // Arrange & Act
        var client = new BillingClient
        {
            Name = "TechStartup Ltda",
            Email = "contato@techstartup.com",
            Document = "12.345.678/0001-90",
            Type = ClientType.Company,
            Status = ClientStatus.Active
        };

        // Assert
        client.Id.Should().NotBeEmpty();
        client.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        client.IsDeleted.Should().BeFalse();
        client.Subscriptions.Should().NotBeNull();
        client.Invoices.Should().NotBeNull();
    }

    [Theory]
    [InlineData(ClientType.Individual)]
    [InlineData(ClientType.Company)]
    public void Client_ShouldAcceptValidTypes(ClientType type)
    {
        // Arrange & Act
        var client = new BillingClient
        {
            Name = "Test Client",
            Email = "test@email.com",
            Document = "123.456.789-00",
            Type = type,
            Status = ClientStatus.Active
        };

        // Assert
        client.Type.Should().Be(type);
    }

    [Theory]
    [InlineData(ClientStatus.Active)]
    [InlineData(ClientStatus.Suspended)]
    [InlineData(ClientStatus.Cancelled)]
    public void Client_ShouldAcceptValidStatuses(ClientStatus status)
    {
        // Arrange & Act
        var client = new BillingClient
        {
            Name = "Test Client",
            Email = "test@email.com",
            Document = "123.456.789-00",
            Type = ClientType.Individual,
            Status = status
        };

        // Assert
        client.Status.Should().Be(status);
    }

    [Fact]
    public void Client_ShouldStoreContactInformation()
    {
        // Arrange & Act
        var client = new BillingClient
        {
            Name = "João Silva",
            Email = "joao@email.com",
            Document = "123.456.789-00",
            Type = ClientType.Individual,
            Status = ClientStatus.Active,
            Phone = "+55 11 98765-4321",
            Address = "Rua Teste, 123",
            City = "São Paulo",
            State = "SP",
            ZipCode = "01234-567",
            Country = "Brasil"
        };

        // Assert
        client.Phone.Should().Be("+55 11 98765-4321");
        client.Address.Should().Be("Rua Teste, 123");
        client.City.Should().Be("São Paulo");
        client.State.Should().Be("SP");
        client.ZipCode.Should().Be("01234-567");
        client.Country.Should().Be("Brasil");
    }
}
