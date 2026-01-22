using PartnershipManager.Domain.Entities.Billing;

namespace PartnershipManager.Tests.Unit.Domain.Billing;

public class PaymentTests
{
    [Fact]
    public void Payment_ShouldInitializeWithDefaultValues()
    {
        // Arrange & Act
        var payment = new Payment
        {
            InvoiceId = Guid.NewGuid(),
            Amount = 299.00m,
            PaymentDate = DateTime.UtcNow,
            PaymentMethod = PaymentMethod.Pix,
            Reference = "PIX-ABC123"
        };

        // Assert
        payment.Id.Should().NotBeEmpty();
        payment.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        payment.IsDeleted.Should().BeFalse();
    }

    [Theory]
    [InlineData(PaymentMethod.BankTransfer)]
    [InlineData(PaymentMethod.CreditCard)]
    [InlineData(PaymentMethod.Pix)]
    [InlineData(PaymentMethod.Boleto)]
    [InlineData(PaymentMethod.Cash)]
    [InlineData(PaymentMethod.Other)]
    public void Payment_ShouldAcceptValidPaymentMethods(PaymentMethod method)
    {
        // Arrange & Act
        var payment = new Payment
        {
            InvoiceId = Guid.NewGuid(),
            Amount = 299.00m,
            PaymentDate = DateTime.UtcNow,
            PaymentMethod = method,
            Reference = "REF-123"
        };

        // Assert
        payment.PaymentMethod.Should().Be(method);
    }

    [Theory]
    [InlineData(99.00)]
    [InlineData(299.00)]
    [InlineData(999.99)]
    public void Payment_ShouldAcceptValidAmounts(decimal amount)
    {
        // Arrange & Act
        var payment = new Payment
        {
            InvoiceId = Guid.NewGuid(),
            Amount = amount,
            PaymentDate = DateTime.UtcNow,
            PaymentMethod = PaymentMethod.Pix,
            Reference = "PIX-123"
        };

        // Assert
        payment.Amount.Should().Be(amount);
    }

    [Fact]
    public void Payment_ShouldStoreReferenceAndNotes()
    {
        // Arrange & Act
        var payment = new Payment
        {
            InvoiceId = Guid.NewGuid(),
            Amount = 299.00m,
            PaymentDate = DateTime.UtcNow,
            PaymentMethod = PaymentMethod.BankTransfer,
            Reference = "TRF-XYZ789",
            Notes = "Pagamento confirmado via extrato bancário"
        };

        // Assert
        payment.Reference.Should().Be("TRF-XYZ789");
        payment.Notes.Should().Be("Pagamento confirmado via extrato bancário");
    }

    [Fact]
    public void Payment_ShouldRecordPaymentDate()
    {
        // Arrange
        var expectedDate = new DateTime(2025, 1, 15);

        // Act
        var payment = new Payment
        {
            InvoiceId = Guid.NewGuid(),
            Amount = 299.00m,
            PaymentDate = expectedDate,
            PaymentMethod = PaymentMethod.Pix,
            Reference = "PIX-123"
        };

        // Assert
        payment.PaymentDate.Should().Be(expectedDate);
    }
}
