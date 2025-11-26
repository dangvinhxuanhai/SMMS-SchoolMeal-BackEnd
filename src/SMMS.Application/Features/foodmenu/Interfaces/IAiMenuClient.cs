using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SMMS.Application.Features.foodmenu.DTOs;

namespace SMMS.Application.Features.foodmenu.Interfaces;
/// <summary>
/// Gateway/service dùng để gọi sang Python AI menu recommender.
/// </summary>
public interface IAiMenuClient
{
    Task<AiMenuRawResponse> RecommendAsync(
        AiMenuRecommendRequest request,
        CancellationToken cancellationToken = default);
}
