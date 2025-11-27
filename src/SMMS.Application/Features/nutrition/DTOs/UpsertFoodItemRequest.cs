using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMMS.Application.Features.nutrition.DTOs;
public class UpsertFoodItemRequest
{
    public string FoodName { get; set; } = default!;
    public string? FoodType { get; set; }
    public string? FoodDesc { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsMainDish { get; set; } = true;
}
