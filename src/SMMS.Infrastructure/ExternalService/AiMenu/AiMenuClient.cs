using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using SMMS.Application.Features.foodmenu.DTOs;
using SMMS.Application.Features.foodmenu.Interfaces;

namespace SMMS.Infrastructure.ExternalService.AiMenu;
public class AiMenuClient : IAiMenuClient
{
    private readonly HttpClient _httpClient;
    private readonly AiMenuOptions _options;
    private readonly JsonSerializerOptions _jsonOptions;

    public AiMenuClient(HttpClient httpClient, IOptions<AiMenuOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = null,          // giữ nguyên snake_case
            PropertyNameCaseInsensitive = true
        };
    }

    public async Task<AiMenuRawResponse> RecommendAsync(
        AiMenuRecommendRequest request,
        CancellationToken cancellationToken = default)
    {
        var json = JsonSerializer.Serialize(request, _jsonOptions);
        using var content = new StringContent(json, Encoding.UTF8, "application/json");

        using var response = await _httpClient.PostAsync(
            _options.RecommendEndpoint,
            content,
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new ApplicationException(
                $"AI recommend failed ({(int)response.StatusCode}): {body}");
        }

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        var result = await JsonSerializer.DeserializeAsync<AiMenuRawResponse>(
            stream, _jsonOptions, cancellationToken);

        if (result == null)
            throw new ApplicationException("AI recommend returned empty body");

        return result;
    }
}
