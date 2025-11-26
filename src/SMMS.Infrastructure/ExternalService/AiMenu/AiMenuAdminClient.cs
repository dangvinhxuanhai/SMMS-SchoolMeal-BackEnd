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

    public async Task RebuildIndexAndGraphAsync(
        Guid schoolId,
        bool rebuildIndex = true,
        bool rebuildGraph = true,
        CancellationToken ct = default)
    {
        var requestBody = new
        {
            school_id = schoolId.ToString(),
            rebuild_index = rebuildIndex,
            rebuild_graph = rebuildGraph
        };

        using var response = await _httpClient.PostAsJsonAsync(
            "/api/v1/admin/rebuild",
            requestBody,
            ct);

        if (!response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync(ct);
            _logger.LogError("AI rebuild failed ({StatusCode}): {Content}",
                response.StatusCode, content);

            // Prod: có thể downgrade thành warning để không chặn CRUD
            throw new ApplicationException(
                $"AI rebuild failed ({(int)response.StatusCode}): {content}");
        }

        // Nếu muốn log response
        var result = await response.Content.ReadAsStringAsync(ct);
        _logger.LogInformation("AI rebuild success for school {SchoolId}: {Result}", schoolId, result);
    }
}
