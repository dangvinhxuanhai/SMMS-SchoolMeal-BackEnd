using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using SMMS.Application.Features.Inventory.DTOs;
using SMMS.Application.Features.Inventory.Interfaces;
using SMMS.Application.Features.Inventory.Queries;

namespace SMMS.Application.Features.Inventory.Handlers;
public class GetInventoryItemByIdQueryHandler
        : IRequestHandler<GetInventoryItemByIdQuery, InventoryItemDto?>
{
    private readonly IInventoryRepository _inventoryRepository;

    public GetInventoryItemByIdQueryHandler(IInventoryRepository inventoryRepository)
    {
        _inventoryRepository = inventoryRepository;
    }

    public async Task<InventoryItemDto?> Handle(
        GetInventoryItemByIdQuery request,
        CancellationToken cancellationToken)
    {
        var item = await _inventoryRepository.GetByIdAsync(
            request.ItemId,
            request.SchoolId,
            cancellationToken);

        if (item == null) return null;

        return new InventoryItemDto
        {
            ItemId = item.ItemId,
            SchoolId = item.SchoolId,
            IngredientId = item.IngredientId,
            IngredientName = item.Ingredient?.IngredientName,
            QuantityGram = item.QuantityGram,
            ExpirationDate = item.ExpirationDate,
            BatchNo = item.BatchNo,
            Origin = item.Origin
        };
    }
}
