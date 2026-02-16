using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using PartnershipManager.Application.DTOs.ClickSign;
using PartnershipManager.Infrastructure.Services;

namespace PartnershipManager.Tests.Unit.Application.Services;

/// <summary>
/// Mock HTTP message handler for testing
/// </summary>
public class MockHttpMessageHandler : HttpMessageHandler
{
    private HttpStatusCode _statusCode;
    private string _responseContent;
    private int _requestCount;

    public MockHttpMessageHandler()
    {
        _statusCode = HttpStatusCode.OK;
        _responseContent = "{}";
        _requestCount = 0;
    }

    public void SetupResponse(HttpStatusCode statusCode, string content)
    {
        _statusCode = statusCode;
        _responseContent = content;
    }

    public int GetRequestCount() => _requestCount;

    public void VerifyRequestCount(int expectedCount)
    {
        _requestCount.Should().Be(expectedCount);
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        _requestCount++;

        return await Task.FromResult(new HttpResponseMessage
        {
            StatusCode = _statusCode,
            Content = new StringContent(_responseContent, System.Text.Encoding.UTF8, "application/vnd.api+json")
        });
    }
}
