using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SMMS.Application.Features.foodmenu.Interfaces;

namespace SMMS.Infrastructure.ExternalService.AiMenu;
// SMMS.Infrastructure/ExternalService/AiMenuAdminClient.cs
public class AiMenuAdminClient : IAiMenuAdminClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AiMenuAdminClient> _logger;
    private readonly AiMenuOptions _options;

    public AiMenuAdminClient(
        HttpClient httpClient,
        IOptions<AiMenuOptions> options,
        ILogger<AiMenuAdminClient> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    /// <summary>
    /// Gửi request sang Python để gen file AI cho tất cả trường NeedRebuildAiIndex = 0.
    /// </summary>
    public async Task RebuildForPendingSchoolsAsync(CancellationToken ct = default)
    {
        // nếu HttpClient chưa có BaseAddress thì có thể đặt ở DI:
        // _httpClient.BaseAddress = new Uri(_options.BaseUrl);

        using var response = await _httpClient.PostAsync(
            "/api/v1/admin/rebuild",
            content: null,  
            cancellationToken: ct);

        var content = await response.Content.ReadAsStringAsync(ct);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("AI rebuild failed ({StatusCode}): {Content}",
                response.StatusCode, content);

            throw new ApplicationException(
                $"AI rebuild failed ({(int)response.StatusCode}): {content}");
        }

        _logger.LogInformation("AI rebuild success for pending schools: {Result}", content);
    }

}

