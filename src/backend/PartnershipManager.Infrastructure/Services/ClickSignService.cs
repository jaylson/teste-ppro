using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PartnershipManager.Application.DTOs.ClickSign;
using PartnershipManager.Application.Interfaces;

namespace PartnershipManager.Infrastructure.Services;

public class ClickSignService : IClickSignService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private readonly HttpClient _httpClient;
    private readonly ILogger<ClickSignService> _logger;
    private readonly string _accessToken;

    public ClickSignService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<ClickSignService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;

        var useSandbox = configuration.GetValue<bool>("ClickSign:UseSandbox");
        var baseUrl = configuration["ClickSign:BaseUrl"];
        _accessToken = configuration["ClickSign:AccessToken"]
            ?? throw new InvalidOperationException("ClickSign AccessToken not configured");

        var resolvedBaseUrl = !string.IsNullOrWhiteSpace(baseUrl)
            ? baseUrl
            : useSandbox
                ? "https://sandbox.clicksign.com/api/v3"
                : "https://app.clicksign.com/api/v3";

        _httpClient.BaseAddress = new Uri(resolvedBaseUrl);
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
        _httpClient.DefaultRequestHeaders.Accept.Clear();
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.api+json"));
    }

    public async Task<ClickSignEnvelopeResponse> CreateEnvelopeAsync(CreateEnvelopeRequest request)
    {
        var payload = new
        {
            data = new
            {
                type = "envelopes",
                attributes = new
                {
                    name = request.Name,
                    sequenceEnabled = request.SequenceEnabled,
                    autoClose = request.AutoClose,
                    remindInterval = request.RemindInterval
                }
            }
        };

        return await SendJsonAsync<ClickSignEnvelopeResponse>(HttpMethod.Post, "/envelopes", payload);
    }

    public async Task<ClickSignDocumentResponse> AddDocumentAsync(string envelopeId, AddDocumentRequest request)
    {
        var payload = new
        {
            data = new
            {
                type = "documents",
                attributes = new
                {
                    filename = request.FileName,
                    contentBase64 = Convert.ToBase64String(request.FileContent)
                }
            }
        };

        return await SendJsonAsync<ClickSignDocumentResponse>(
            HttpMethod.Post,
            $"/envelopes/{envelopeId}/documents",
            payload);
    }

    public async Task<ClickSignSignerResponse> AddSignerAsync(string envelopeId, AddSignerRequest request)
    {
        var payload = new
        {
            data = new
            {
                type = "signers",
                attributes = new
                {
                    name = request.Name,
                    email = request.Email,
                    phoneNumber = request.PhoneNumber,
                    documentation = request.DocumentNumber
                }
            }
        };

        return await SendJsonAsync<ClickSignSignerResponse>(
            HttpMethod.Post,
            $"/envelopes/{envelopeId}/signers",
            payload);
    }

    public async Task CreateRequirementsAsync(string envelopeId, CreateRequirementsRequest request)
    {
        var qualificationPayload = new
        {
            data = new
            {
                type = "requirements",
                attributes = new
                {
                    action = request.Role,
                    role = request.Role
                },
                relationships = new
                {
                    signer = new { data = new { id = request.SignerId, type = "signers" } },
                    document = new { data = new { id = request.DocumentId, type = "documents" } }
                }
            }
        };

        await SendJsonAsync<object>(
            HttpMethod.Post,
            $"/envelopes/{envelopeId}/requirements",
            qualificationPayload);

        var authPayload = new
        {
            data = new
            {
                type = "requirements",
                attributes = new
                {
                    action = "auth",
                    auth = request.AuthMethod
                },
                relationships = new
                {
                    signer = new { data = new { id = request.SignerId, type = "signers" } },
                    document = new { data = new { id = request.DocumentId, type = "documents" } }
                }
            }
        };

        await SendJsonAsync<object>(
            HttpMethod.Post,
            $"/envelopes/{envelopeId}/requirements",
            authPayload);
    }

    public async Task StartEnvelopeAsync(string envelopeId)
    {
        var payload = new
        {
            data = new
            {
                type = "envelopes",
                id = envelopeId,
                attributes = new
                {
                    status = "running"
                }
            }
        };

        await SendJsonAsync<object>(HttpMethod.Patch, $"/envelopes/{envelopeId}", payload);
    }

    public async Task<ClickSignEnvelopeResponse> GetEnvelopeAsync(string envelopeId)
    {
        return await SendJsonAsync<ClickSignEnvelopeResponse>(
            HttpMethod.Get,
            $"/envelopes/{envelopeId}");
    }

    public async Task<byte[]> DownloadDocumentAsync(string envelopeId, string documentId)
    {
        var response = await _httpClient.GetAsync($"/envelopes/{envelopeId}/documents/{documentId}/download");
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            _logger.LogError("ClickSign download failed: {Status} {Body}", response.StatusCode, body);
            throw new InvalidOperationException("ClickSign download failed");
        }

        return await response.Content.ReadAsByteArrayAsync();
    }

    private async Task<T> SendJsonAsync<T>(HttpMethod method, string url, object? payload = null)
    {
        using var request = new HttpRequestMessage(method, url);
        if (payload != null)
        {
            var json = JsonSerializer.Serialize(payload, JsonOptions);
            request.Content = new StringContent(json, Encoding.UTF8, "application/vnd.api+json");
        }

        var response = await _httpClient.SendAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            _logger.LogError("ClickSign request failed: {Status} {Body}", response.StatusCode, body);
            throw new InvalidOperationException("ClickSign request failed");
        }

        if (typeof(T) == typeof(object))
        {
            return default!;
        }

        var responseBody = await response.Content.ReadAsStringAsync();
        var parsed = JsonSerializer.Deserialize<T>(responseBody, JsonOptions);
        if (parsed == null)
        {
            throw new InvalidOperationException("Invalid ClickSign response");
        }

        return parsed;
    }
}
