using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using SMMS.Application.Features.foodmenu.DTOs;
using SMMS.Application.Features.Inventory.DTOs;

namespace SMMS.Application.Features.Inventory.Queries;
public sealed record GetInventoryItemsQuery(
        Guid SchoolId,
        int PageIndex = 1,
        int PageSize = 20
    ) : IRequest<PagedResult<InventoryItemDto>>;
