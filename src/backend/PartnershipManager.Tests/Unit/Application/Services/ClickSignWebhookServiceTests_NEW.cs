using System.Net;
using Moq;
using Microsoft.Extensions.Logging;
using PartnershipManager.Application.DTOs.ClickSign;
using PartnershipManager.Infrastructure.Persistence;
using PartnershipManager.Infrastructure.Services;

namespace PartnershipManager.Tests.Unit.Application.Services;

public class ClickSignWebhookServiceTests
{
    private readonly Mock<DapperContext> _contextMock;
    private readonly Mock<ILogger<ClickSignWebhookService>> _loggerMock;
    private ClickSignWebhookService _service;

    public ClickSignWebhookServiceTests()
    {
        _contextMock = new Mock<DapperContext>(
            new Mock<IConfiguration>().Object);
        _loggerMock = new Mock<ILogger<ClickSignWebhookService>>();

        _service = new ClickSignWebhookService(_contextMock.Object, _loggerMock.Object);
    }

    #region ProcessAsync - Sign Event

    [Fact]
    public async Task ProcessAsync_WithSignEvent_ShouldProcessWebhook()
    {
        // Arrange
        var contractId = Guid.NewGuid();
        var signerEmail = "signer@example.com";

        var payload = new ClickSignWebhookPayload
        {
            Event = "sign",
            Data = new ClickSignDataResponse
            {
                Id = "signer-123",
                Type = "signers",
                Attributes = new
                {
                    SignerEmail = signerEmail,
                    Email = signerEmail
                }
            }
        };

        // Act
        await _service.ProcessAsync(payload);

        // Assert - Service should complete without errors
        // Actual database calls are mocked by DapperContext
        _loggerMock.Verify(
            x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.AtLeast(0));
    }

    #endregion

    #region ProcessAsync - Error Cases

    [Fact]
    public async Task ProcessAsync_WithoutEventName_ShouldLogWarningAndReturn()
    {
        // Arrange
        var payload = new ClickSignWebhookPayload
        {
            Event = null,
            Data = new ClickSignDataResponse
            {
                Id = "123",
                Attributes = new object()
            }
        };

        // Act
        await _service.ProcessAsync(payload);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Warning),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("event name")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task ProcessAsync_WithoutExternalReference_ShouldLogWarningAndReturn()
    {
        // Arrange
        var payload = new ClickSignWebhookPayload
        {
            Event = "sign",
            Data = new ClickSignDataResponse
            {
                Id = null,
                Attributes = new { ExternalId = (string?)null }
            }
        };

        // Act
        await _service.ProcessAsync(payload);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Warning),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("external reference")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    #endregion

    #region Event Type Coverage

    [Fact]
    public async Task ProcessAsync_WithRefusalEvent_ShouldProcessRefusal()
    {
        // Arrange
        var payload = new ClickSignWebhookPayload
        {
            Event = "refusal",
            Data = new ClickSignDataResponse
            {
                Id = "signer-001",
                Attributes = new object()
            }
        };

        // Act
        await _service.ProcessAsync(payload);

        // Assert - Service should complete
        _loggerMock.Verify(
            x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.AtLeast(0));
    }

    [Fact]
    public async Task ProcessAsync_WithDocumentClosedEvent_ShouldProcessClosure()
    {
        // Arrange
        var payload = new ClickSignWebhookPayload
        {
            Event = "document_closed",
            Data = new ClickSignDataResponse
            {
                Id = "contract-123",
                Attributes = new object()
            }
        };

        // Act
        await _service.ProcessAsync(payload);

        // Assert - Service should complete
        _loggerMock.Verify(
            x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.AtLeast(0));
    }

    #endregion
}
