using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using SMMS.Application.Common.Interfaces;
using SMMS.Application.Common.Options;

namespace SMMS.Infrastructure.Service;
public sealed class PayOsIntegrationService : IPayOsIntegrationService
{
    private readonly HttpClient _httpClient;
    private readonly PayOsOptions _options;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public PayOsIntegrationService(
        HttpClient httpClient,
        IOptions<PayOsOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;

        if (!Uri.TryCreate(_options.BaseUrl, UriKind.Absolute, out var baseUri))
        {
            throw new InvalidOperationException("PayOS BaseUrl config is invalid.");
        }

        _httpClient.BaseAddress = baseUri;
    }

    public async Task ConfirmWebhookAsync(
        string clientId,
        string apiKey,
        string webhookUrl,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(webhookUrl))
        {
            throw new InvalidOperationException("WebhookUrl is not configured for PayOS.");
        }

        var requestBody = new { webhookUrl };

        using var requestMessage = new HttpRequestMessage(
            HttpMethod.Post,
            "confirm-webhook"); // https://api-merchant.payos.vn/confirm-webhook :contentReference[oaicite:0]{index=0}

        requestMessage.Headers.Add("x-client-id", clientId);
        requestMessage.Headers.Add("x-api-key", apiKey);
        requestMessage.Content = new StringContent(
            JsonSerializer.Serialize(requestBody, JsonOptions),
            Encoding.UTF8,
            "application/json");

        using var response = await _httpClient.SendAsync(requestMessage, cancellationToken);
        var content = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(
                $"PayOS confirm-webhook failed with status {(int)response.StatusCode}: {content}");
        }

        var payload = JsonSerializer.Deserialize<PayOsConfirmWebhookResponse>(content, JsonOptions);

        if (payload is null || !string.Equals(payload.Code, "00", StringComparison.OrdinalIgnoreCase))
        {
            var desc = payload?.Desc ?? "Unknown error";
            throw new InvalidOperationException($"PayOS confirm-webhook failed: {desc}");
        }
    }

    private sealed class PayOsConfirmWebhookResponse
    {
        public string Code { get; set; } = string.Empty;
        public string Desc { get; set; } = string.Empty;
    }
}
