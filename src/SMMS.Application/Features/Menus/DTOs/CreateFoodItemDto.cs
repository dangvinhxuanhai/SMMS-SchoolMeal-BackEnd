using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMMS.Application.Features.Menus.DTOs;
public class CreateFoodItemDto
{
    public string FoodName { get; set; } = null!;
    public string? FoodType { get; set; }
    public string? FoodDesc { get; set; }
    public string? ImageUrl { get; set; }
    public Guid SchoolId { get; set; }
    public Guid? CreatedBy { get; set; }
    public bool IsActive { get; set; } = true;
}

