using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using SMMS.Application.Abstractions;
using SMMS.Application.Features.Inventory.Commands;
using SMMS.Application.Features.Inventory.Interfaces;

namespace SMMS.Application.Features.Inventory.Handlers;
public class UpdateInventoryItemCommandHandler
        : IRequestHandler<UpdateInventoryItemCommand>
{
    private readonly IInventoryRepository _inventoryRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateInventoryItemCommandHandler(
        IInventoryRepository inventoryRepository,
        IUnitOfWork unitOfWork)
    {
        _inventoryRepository = inventoryRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(
        UpdateInventoryItemCommand request,
        CancellationToken cancellationToken)
    {
        var item = await _inventoryRepository.GetByIdAsync(
            request.ItemId,
            request.SchoolId,
            cancellationToken);

        if (item == null)
            throw new KeyNotFoundException("Inventory item not found.");

        if (request.QuantityGram.HasValue)
            item.QuantityGram = request.QuantityGram.Value;

        if (request.ExpirationDate.HasValue)
            item.ExpirationDate = request.ExpirationDate.Value;

        if (request.BatchNo != null)
            item.BatchNo = request.BatchNo;

        if (request.Origin != null)
            item.Origin = request.Origin;

        item.LastUpdated = DateTime.UtcNow;

        await _inventoryRepository.UpdateAsync(item, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

    }
}
