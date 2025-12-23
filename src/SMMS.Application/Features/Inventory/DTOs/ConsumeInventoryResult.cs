using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMMS.Application.Features.Inventory.DTOs;
public class ConsumeInventoryResult
{
    public bool IsSuccess { get; set; }
    public List<InventoryConsumeItemResult> Items { get; set; } = new();
    public string? Warning { get; set; }
}

public class InventoryConsumeItemResult
{
    public int IngredientId { get; set; }
    public string IngredientName { get; set; } = null!;
    public decimal RequiredGram { get; set; }
    public decimal AvailableGram { get; set; }
    public decimal ConsumedGram { get; set; }
    public bool IsOverConsumed { get; set; }
}
