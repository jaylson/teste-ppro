using PartnershipManager.Domain.Entities.Billing;

namespace PartnershipManager.Tests.Unit.Domain.Billing;

public class InvoiceTests
{
    [Fact]
    public void Invoice_ShouldInitializeWithDefaultValues()
    {
        // Arrange & Act
        var invoice = new Invoice
        {
            ClientId = Guid.NewGuid(),
            InvoiceNumber = "INV-001",
            Amount = 299.00m,
            IssueDate = DateTime.UtcNow,
            DueDate = DateTime.UtcNow.AddDays(30),
            Status = InvoiceStatus.Pending
        };

        // Assert
        invoice.Id.Should().NotBeEmpty();
        invoice.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        invoice.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public void MarkAsPaid_ShouldChangeStatusToPaid()
    {
        // Arrange
        var invoice = new Invoice
        {
            ClientId = Guid.NewGuid(),
            InvoiceNumber = "INV-001",
            Amount = 299.00m,
            IssueDate = DateTime.UtcNow,
            DueDate = DateTime.UtcNow.AddDays(30),
            Status = InvoiceStatus.Pending
        };
        var paymentDate = DateTime.UtcNow;

        // Act
        invoice.MarkAsPaid(paymentDate);

        // Assert
        invoice.Status.Should().Be(InvoiceStatus.Paid);
        invoice.UpdatedAt.Should().Be(paymentDate);
    }

    [Fact]
    public void MarkAsOverdue_ShouldChangeStatusToOverdue_WhenDueDatePassed()
    {
        // Arrange
        var invoice = new Invoice
        {
            ClientId = Guid.NewGuid(),
            InvoiceNumber = "INV-001",
            Amount = 299.00m,
            IssueDate = DateTime.UtcNow.AddDays(-35),
            DueDate = DateTime.UtcNow.AddDays(-5), // Vencida há 5 dias
            Status = InvoiceStatus.Pending
        };

        // Act
        invoice.MarkAsOverdue();

        // Assert
        invoice.Status.Should().Be(InvoiceStatus.Overdue);
    }

    [Fact]
    public void MarkAsOverdue_ShouldNotChangeStatus_WhenStatusIsNotPending()
    {
        // Arrange
        var invoice = new Invoice
        {
            ClientId = Guid.NewGuid(),
            InvoiceNumber = "INV-001",
            Amount = 299.00m,
            IssueDate = DateTime.UtcNow.AddDays(-35),
            DueDate = DateTime.UtcNow.AddDays(-5),
            Status = InvoiceStatus.Paid // Já está paga
        };

        // Act
        invoice.MarkAsOverdue();

        // Assert
        invoice.Status.Should().Be(InvoiceStatus.Paid);
    }

    [Fact]
    public void MarkAsOverdue_ShouldNotChangeStatus_WhenDueDateNotPassed()
    {
        // Arrange
        var invoice = new Invoice
        {
            ClientId = Guid.NewGuid(),
            InvoiceNumber = "INV-001",
            Amount = 299.00m,
            IssueDate = DateTime.UtcNow,
            DueDate = DateTime.UtcNow.AddDays(30), // Ainda não venceu
            Status = InvoiceStatus.Pending
        };

        // Act
        invoice.MarkAsOverdue();

        // Assert
        invoice.Status.Should().Be(InvoiceStatus.Pending);
    }

    [Fact]
    public void Cancel_ShouldChangeStatusToCancelled()
    {
        // Arrange
        var invoice = new Invoice
        {
            ClientId = Guid.NewGuid(),
            InvoiceNumber = "INV-001",
            Amount = 299.00m,
            IssueDate = DateTime.UtcNow,
            DueDate = DateTime.UtcNow.AddDays(30),
            Status = InvoiceStatus.Pending
        };
        var beforeUpdate = invoice.UpdatedAt;

        // Act
        System.Threading.Thread.Sleep(10);
        invoice.Cancel();

        // Assert
        invoice.Status.Should().Be(InvoiceStatus.Cancelled);
        invoice.UpdatedAt.Should().BeAfter(beforeUpdate);
    }

    [Theory]
    [InlineData(100.00)]
    [InlineData(299.00)]
    [InlineData(999.99)]
    public void Invoice_ShouldAcceptValidAmounts(decimal amount)
    {
        // Arrange & Act
        var invoice = new Invoice
        {
            ClientId = Guid.NewGuid(),
            InvoiceNumber = "INV-001",
            Amount = amount,
            IssueDate = DateTime.UtcNow,
            DueDate = DateTime.UtcNow.AddDays(30),
            Status = InvoiceStatus.Pending
        };

        // Assert
        invoice.Amount.Should().Be(amount);
    }
}
