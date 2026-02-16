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
            new Mock<Microsoft.Extensions.Configuration.IConfiguration>().Object);
        _loggerMock = new Mock<ILogger<ClickSignWebhookService>>();

        _service = new ClickSignWebhookService(_contextMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task ProcessAsync_WithoutEventName_ShouldLogWarningAndReturn()
    {
        // Arrange
        var payload = new ClickSignWebhookPayload
        {
            Event = null!,
            Data = null,
            EventDate = DateTime.UtcNow
        };

        // Act
        await _service.ProcessAsync(payload);

        // Assert - Service should complete without errors
        _loggerMock.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Warning),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task ProcessAsync_WithoutExternalReference_ShouldLogWarningAndReturn()
    {
        // Arrange
        var payload = new ClickSignWebhookPayload
        {
            Event = "sign",
            Data = new ClickSignData { Id = null, Data = new List<object>() },
            EventDate = DateTime.UtcNow
        };

        // Act
        await _service.ProcessAsync(payload);

        // Assert - Service should complete without errors
        _loggerMock.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Warning),
               It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task ProcessAsync_ProcessingCompletes_WithoutThrows()
    {
        // Arrange - Setup with valid but unknown contracts
        var payload = new ClickSignWebhookPayload
        {
            Event = "sign",
            Data = new ClickSignData 
            { 
                Id = "test-123", 
                Data = new List<object>() 
            },
            EventDate = DateTime.UtcNow
        };

        // Act & Assert - Should not throw
        var task = _service.ProcessAsync(payload);
        await task;
    }
}
