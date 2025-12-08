using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMMS.Application.Features.foodmenu.DTOs;
public sealed class ScheduledFoodIngredientDto
{
    public int IngredientId { get; set; }
    public string IngredientName { get; set; } = default!;
    public decimal QuantityGram { get; set; }
}
