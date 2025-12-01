using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;

namespace SMMS.Application.Features.Inventory.Commands;
public class UpdateInventoryItemCommand : IRequest
{
    public Guid SchoolId { get; set; }
    public int ItemId { get; set; }

    // các field cho phép sửa
    public decimal? QuantityGram { get; set; }
    public DateOnly? ExpirationDate { get; set; }
    public string? BatchNo { get; set; }
    public string? Origin { get; set; }
}
