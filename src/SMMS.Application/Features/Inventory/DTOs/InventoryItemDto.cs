using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMMS.Application.Features.Inventory.DTOs;
public class InventoryItemDto
{
    public int ItemId { get; set; }
    public Guid SchoolId { get; set; }

    public int IngredientId { get; set; }
    public string? IngredientName { get; set; }

    public decimal QuantityGram { get; set; }
    public DateOnly? ExpirationDate { get; set; }
    public string? BatchNo { get; set; }
    public string? Origin { get; set; }
}
