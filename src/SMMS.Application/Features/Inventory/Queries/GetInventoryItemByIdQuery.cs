using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using SMMS.Application.Features.Inventory.DTOs;

namespace SMMS.Application.Features.Inventory.Queries;
public sealed record GetInventoryItemByIdQuery(
        Guid SchoolId,
        int ItemId
    ) : IRequest<InventoryItemDto?>;
