using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using SMMS.Application.Features.Plan.Commands;
using SMMS.Application.Features.Plan.DTOs;
using SMMS.Application.Features.Plan.Interfaces;
using SMMS.Application.Features.Plan.Queries;
using SMMS.Domain.Entities.purchasing;

namespace SMMS.Application.Features.Plan.Handlers;
public class PurchaseOrderHandler :
        IRequestHandler<CreatePurchaseOrderFromPlanCommand, PurchaseOrderDetailDto>,
        IRequestHandler<UpdatePurchaseOrderHeaderCommand, PurchaseOrderDetailDto>,
        IRequestHandler<DeletePurchaseOrderCommand, Unit>,
        IRequestHandler<GetPurchaseOrderByIdQuery, PurchaseOrderDetailDto?>,
        IRequestHandler<GetPurchaseOrdersBySchoolQuery, List<PurchaseOrderSummaryDto>>,
        IRequestHandler<UpdatePurchaseOrderLinesCommand, List<PurchaseOrderLineDto>>,
        IRequestHandler<DeletePurchaseOrderLineCommand, Unit>
{
    private readonly IPurchaseOrderRepository _repository;

    public PurchaseOrderHandler(IPurchaseOrderRepository repository)
    {
        _repository = repository;
    }

    public async Task<PurchaseOrderDetailDto> Handle(
        CreatePurchaseOrderFromPlanCommand request,
        CancellationToken cancellationToken)
    {
        var order = await _repository.CreateFromPlanAsync(
            request.PlanId,
            request.SchoolId,
            request.StaffId,
            request.SupplierName,
            request.Note,
            request.OrderDate,
            request.Status,
            cancellationToken);

        return MapToDetail(order);
    }

    public async Task<PurchaseOrderDetailDto> Handle(
        UpdatePurchaseOrderHeaderCommand request,
        CancellationToken cancellationToken)
    {
        await _repository.UpdateOrderHeaderAsync(
            request.OrderId,
            request.SchoolId,
            request.SupplierName,
            request.Note,
            request.OrderDate,
            request.Status,
            cancellationToken);

        var order = await _repository.GetByIdAsync(
            request.OrderId, request.SchoolId, cancellationToken)
            ?? throw new Exception("Purchase order not found after update.");

        return MapToDetail(order);
    }

    public async Task<Unit> Handle(
        DeletePurchaseOrderCommand request,
        CancellationToken cancellationToken)
    {
        await _repository.DeleteOrderAsync(
            request.OrderId, request.SchoolId, cancellationToken);
        return Unit.Value;
    }

    public async Task<PurchaseOrderDetailDto?> Handle(
        GetPurchaseOrderByIdQuery request,
        CancellationToken cancellationToken)
    {
        var order = await _repository.GetByIdAsync(
            request.OrderId, request.SchoolId, cancellationToken);

        return order == null ? null : MapToDetail(order);
    }

    public async Task<List<PurchaseOrderSummaryDto>> Handle(
        GetPurchaseOrdersBySchoolQuery request,
        CancellationToken cancellationToken)
    {
        var orders = await _repository.GetListAsync(
            request.SchoolId, request.FromDate, request.ToDate, cancellationToken);

        return orders.Select(MapToSummary).ToList();
    }

    public async Task<List<PurchaseOrderLineDto>> Handle(
        UpdatePurchaseOrderLinesCommand request,
        CancellationToken cancellationToken)
    {
        await _repository.UpdateOrderLinesAsync(
            request.OrderId,
            request.SchoolId,
            request.Lines,
            request.UserId,
            cancellationToken);

        var lines = await _repository.GetOrderLinesAsync(
            request.OrderId,
            request.SchoolId,
            cancellationToken);

        return lines.Select(MapToLine).ToList();
    }

    public async Task<Unit> Handle(
        DeletePurchaseOrderLineCommand request,
        CancellationToken cancellationToken)
    {
        await _repository.DeleteOrderLineAsync(
            request.LinesId,
            request.OrderId,
            request.SchoolId,
            cancellationToken);

        return Unit.Value;
    }

    #region Mapping helpers

    private static PurchaseOrderDetailDto MapToDetail(PurchaseOrder order)
    {
        return new PurchaseOrderDetailDto
        {
            OrderId = order.OrderId,
            SchoolId = order.SchoolId,
            OrderDate = order.OrderDate,
            PurchaseOrderStatus = order.PurchaseOrderStatus,
            SupplierName = order.SupplierName,
            Note = order.Note,
            PlanId = order.PlanId,
            StaffInCharged = order.StaffInCharged,
            Lines = order.PurchaseOrderLines?
                .Select(MapToLine)
                .ToList() ?? new List<PurchaseOrderLineDto>()
        };
    }

    private static PurchaseOrderSummaryDto MapToSummary(PurchaseOrder order)
    {
        var totalQty = order.PurchaseOrderLines?.Sum(l => l.QuantityGram) ?? 0;
        var count = order.PurchaseOrderLines?.Count ?? 0;

        return new PurchaseOrderSummaryDto
        {
            OrderId = order.OrderId,
            OrderDate = order.OrderDate,
            PurchaseOrderStatus = order.PurchaseOrderStatus,
            SupplierName = order.SupplierName,
            PlanId = order.PlanId,
            LinesCount = count,
            TotalQuantityGram = totalQty
        };
    }

    private static PurchaseOrderLineDto MapToLine(PurchaseOrderLine line)
    {
        return new PurchaseOrderLineDto
        {
            LinesId = line.LinesId,
            IngredientId = line.IngredientId,
            IngredientName = line.Ingredient?.IngredientName ?? string.Empty,
            QuantityGram = line.QuantityGram,
            UnitPrice = line.UnitPrice,
            BatchNo = line.BatchNo,
            Origin = line.Origin,
            ExpiryDate = line.ExpiryDate.HasValue ? line.ExpiryDate.Value.ToDateTime(TimeOnly.MinValue) : (DateTime?)null
        };
    }

    #endregion
}
