using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using SMMS.Application.Features.foodmenu.DTOs;
using SMMS.Application.Features.Inventory.DTOs;
using SMMS.Application.Features.Inventory.Interfaces;
using SMMS.Application.Features.Inventory.Queries;

namespace SMMS.Application.Features.Inventory.Handlers;
public class GetInventoryItemsQueryHandler
        : IRequestHandler<GetInventoryItemsQuery, PagedResult<InventoryItemDto>>
{
    private readonly IInventoryRepository _inventoryRepository;

    public GetInventoryItemsQueryHandler(IInventoryRepository inventoryRepository)
    {
        _inventoryRepository = inventoryRepository;
    }

    public async Task<PagedResult<InventoryItemDto>> Handle(
        GetInventoryItemsQuery request,
        CancellationToken cancellationToken)
    {
        var total = await _inventoryRepository.CountBySchoolAsync(
            request.SchoolId, cancellationToken);

        var result = new PagedResult<InventoryItemDto>
        {
            PageIndex = request.PageIndex,
            PageSize = request.PageSize,
            TotalCount = total
        };

        if (total == 0)
            return result;

        var items = await _inventoryRepository.GetPagedBySchoolAsync(
            request.SchoolId,
            request.PageIndex,
            request.PageSize,
            cancellationToken);

        result.Items = items.Select(i => new InventoryItemDto
        {
            ItemId = i.ItemId,
            SchoolId = i.SchoolId,
            IngredientId = i.IngredientId,
            IngredientName = i.Ingredient?.IngredientName,
            QuantityGram = i.QuantityGram,
            ExpirationDate = i.ExpirationDate,
            BatchNo = i.BatchNo,
            Origin = i.Origin
        }).ToList();

        return result;
    }
}
