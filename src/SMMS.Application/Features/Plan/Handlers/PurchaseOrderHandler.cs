using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using SMMS.Application.Abstractions;
using SMMS.Application.Features.Inventory.Interfaces;
using SMMS.Application.Features.Manager.DTOs;
using SMMS.Application.Features.Plan.Commands;
using SMMS.Application.Features.Plan.DTOs;
using SMMS.Application.Features.Plan.Interfaces;
using SMMS.Application.Features.Plan.Queries;
using SMMS.Domain.Entities.purchasing;

namespace SMMS.Application.Features.Plan.Handlers;
public class PurchaseOrderHandler :
        IRequestHandler<CreatePurchaseOrderFromPlanCommand, KsPurchaseOrderDto>,
        IRequestHandler<UpdatePurchaseOrderHeaderCommand, KsPurchaseOrderDetailDto>,
        IRequestHandler<DeletePurchaseOrderCommand, Unit>,
        IRequestHandler<GetPurchaseOrderByIdQuery, KsPurchaseOrderDetailDto?>,
        IRequestHandler<GetPurchaseOrdersBySchoolQuery, List<PurchaseOrderSummaryDto>>,
        IRequestHandler<UpdatePurchaseOrderLinesCommand, List<KsPurchaseOrderLineDto>>,
        IRequestHandler<DeletePurchaseOrderLineCommand, Unit>,
        IRequestHandler<ConfirmPurchaseOrderCommand, KsPurchaseOrderDetailDto>,
        IRequestHandler<RejectPurchaseOrderCommand, KsPurchaseOrderDetailDto>
{
    private readonly IPurchaseOrderRepository _repository;
    private readonly IPurchasePlanRepository _planRepository;
    private readonly IInventoryRepository _inventoryRepository;
    private readonly IUnitOfWork _unitOfWork;
    public PurchaseOrderHandler(IPurchaseOrderRepository repository, IUnitOfWork unitOfWork, IPurchasePlanRepository planRepository, IInventoryRepository inventoryRepository)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _planRepository = planRepository;
        _inventoryRepository = inventoryRepository;
    }

    public async Task<KsPurchaseOrderDto> Handle(
                CreatePurchaseOrderFromPlanCommand request,
                CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.SupplierName))
            throw new ArgumentException("SupplierName is required", nameof(request.SupplierName));

        if (request.Lines == null || request.Lines.Count == 0)
            throw new ArgumentException("At least one line is required", nameof(request.Lines));

        // 1) Load plan
        var plan = await _planRepository.GetByIdAsync(request.PlanId, cancellationToken)
                   ?? throw new KeyNotFoundException($"PurchasePlan {request.PlanId} not found");

        // Optional: chỉ cho phép tạo order từ plan đã Confirmed
        //if (!string.Equals(plan.PlanStatus, "Confirmed", StringComparison.OrdinalIgnoreCase))
        //{
        //    throw new InvalidOperationException("Purchase plan must be Confirmed before creating purchase order.");
        //}

        // 2) Không cho tạo trùng đơn cho cùng 1 Plan
        if (await _repository.ExistsForPlanAsync(plan.PlanId, cancellationToken))
            throw new InvalidOperationException($"Purchase order already exists for Plan {plan.PlanId}");

        // 3) Lấy SchoolId qua ScheduleMeal
        var schoolId = await _planRepository.GetSchoolIdAsync(plan.PlanId, cancellationToken);

        // 4) Lấy tất cả plan lines
        var planLines = await _planRepository.GetLinesAsync(plan.PlanId, cancellationToken);
        if (planLines.Count == 0)
            throw new InvalidOperationException($"Purchase plan {plan.PlanId} has no lines.");

        // Map request line theo IngredientId để tra nhanh
        var lineDict = request.Lines
            .ToDictionary(x => x.IngredientId);

        // 5) Tạo PurchaseOrder + Lines (gắn qua navigation, EF tự set FK)
        var order = new PurchaseOrder
        {
            SchoolId = schoolId,
            OrderDate = DateTime.UtcNow,
            PurchaseOrderStatus = "Draft",       // hoặc "Created" tùy convention
            SupplierName = request.SupplierName.Trim(),
            Note = request.Note,
            PlanId = plan.PlanId,
            StaffInCharged = request.StaffUserId,
            PurchaseOrderLines = new List<PurchaseOrderLine>()
        };

        foreach (var pl in planLines)
        {
            if (!lineDict.TryGetValue(pl.IngredientId, out var lineCfg))
            {
                // Bạn có thể đổi thành "continue" nếu muốn cho phép bỏ qua 1 số Ingredient
                throw new InvalidOperationException(
                    $"Missing price info for IngredientId {pl.IngredientId} in request.");
            }

            var quantity = lineCfg.QuantityOverrideGram ?? pl.RqQuanityGram;

            var orderLine = new PurchaseOrderLine
            {
                IngredientId = pl.IngredientId,
                QuantityGram = quantity,
                UnitPrice = lineCfg.UnitPrice,
                BatchNo = lineCfg.BatchNo,
                Origin = lineCfg.Origin,
                ExpiryDate = lineCfg.ExpiryDate.HasValue ? DateOnly.FromDateTime(lineCfg.ExpiryDate.Value) : null,
                UserId = request.StaffUserId
            };

            order.PurchaseOrderLines.Add(orderLine);
        }

        // 6) Lưu
        await _repository.AddAsync(order, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // 7) Map sang DTO trả về
        var dto = new KsPurchaseOrderDto
        {
            OrderId = order.OrderId,
            SchoolId = order.SchoolId,
            OrderDate = order.OrderDate,
            PurchaseOrderStatus = order.PurchaseOrderStatus,
            SupplierName = order.SupplierName,
            Note = order.Note,
            PlanId = order.PlanId,
            StaffInCharged = order.StaffInCharged,
            Lines = order.PurchaseOrderLines
                .Select(l => new KsPurchaseOrderLineDto
                {
                    LinesId = l.LinesId,
                    IngredientId = l.IngredientId,
                    QuantityGram = l.QuantityGram,
                    UnitPrice = l.UnitPrice,
                    BatchNo = l.BatchNo,
                    Origin = l.Origin,
                    ExpiryDate = l.ExpiryDate.HasValue ? l.ExpiryDate.Value.ToDateTime(TimeOnly.MinValue) : null
                })
                .ToList()
        };

        return dto;
    }

    public async Task<KsPurchaseOrderDetailDto> Handle(
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

    public async Task<KsPurchaseOrderDetailDto?> Handle(
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

    public async Task<List<KsPurchaseOrderLineDto>> Handle(
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

    public async Task<KsPurchaseOrderDetailDto> Handle(
    ConfirmPurchaseOrderCommand request,
    CancellationToken cancellationToken)
    {
        // 1) Load order (kèm lines)
        var order = await _repository.GetByIdAsync(
            request.OrderId,
            request.SchoolId,
            cancellationToken)
            ?? throw new KeyNotFoundException("Purchase order not found.");

        if (!string.Equals(order.PurchaseOrderStatus, "Draft", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Only Draft purchase orders can be confirmed.");
        }

        if (order.PlanId == null)
            throw new InvalidOperationException("Purchase order is not linked to a purchase plan.");

        // 2) Load plan + schedule meal
        var plan = await _planRepository.GetByIdAsync(order.PlanId.Value, cancellationToken)
                  ?? throw new KeyNotFoundException($"Purchase plan {order.PlanId} not found.");

        // Giả sử GetByIdAsync include luôn ScheduleMeal navigation.
        plan.PlanStatus = "Confirmed";
        plan.ConfirmedBy = request.ManagerUserId;
        plan.ConfirmedAt = DateTime.UtcNow;

        if (plan.ScheduleMeal != null)
        {
            plan.ScheduleMeal.Status = "Confirmed";
        }

        // 3) Đổi status order
        order.PurchaseOrderStatus = "Confirmed";

        // 4) Cập nhật Inventory từ các purchase order lines
        var reference = $"PO-FromPlan:{plan.PlanId}";

        foreach (var line in order.PurchaseOrderLines)
        {
            var item = await _inventoryRepository.AddOrIncreaseAsync(
                order.SchoolId,
                line.IngredientId,
                line.QuantityGram,
                line.ExpiryDate,
                line.BatchNo,
                line.Origin,
                request.ManagerUserId,    // CreatedBy = manager duyệt
                cancellationToken);

            await _inventoryRepository.AddInboundTransactionAsync(
                item,
                line.QuantityGram,
                reference,
                cancellationToken);
        }

        // 5) Lưu tất cả trong 1 transaction
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDetail(order);
    }

    public async Task<KsPurchaseOrderDetailDto> Handle(
    RejectPurchaseOrderCommand request,
    CancellationToken cancellationToken)
    {
        var order = await _repository.GetByIdAsync(
            request.OrderId,
            request.SchoolId,
            cancellationToken)
            ?? throw new KeyNotFoundException("Purchase order not found.");

        if (!string.Equals(order.PurchaseOrderStatus, "Draft", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Only Draft purchase orders can be exported.");
        }

        if (order.PlanId == null)
            throw new InvalidOperationException("Purchase order is not linked to a purchase plan.");

        var plan = await _planRepository.GetByIdAsync(order.PlanId.Value, cancellationToken)
                  ?? throw new KeyNotFoundException($"Purchase plan {order.PlanId} not found.");

        // Đổi status sang Exported, không nhập kho
        order.PurchaseOrderStatus = "Rejected";
        plan.PlanStatus = "Rejected";

        if (plan.ScheduleMeal != null)
        {
            plan.ScheduleMeal.Status = "Rejected";
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDetail(order);
    }

    #region Mapping helpers

    private static KsPurchaseOrderDetailDto MapToDetail(PurchaseOrder order)
    {
        return new KsPurchaseOrderDetailDto
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
                .ToList() ?? new List<KsPurchaseOrderLineDto>()
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

    private static KsPurchaseOrderLineDto MapToLine(PurchaseOrderLine line)
    {
        return new KsPurchaseOrderLineDto
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
