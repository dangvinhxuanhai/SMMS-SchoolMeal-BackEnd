using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using MediatR;
using SMMS.Application.Features.nutrition.DTOs;

namespace SMMS.Application.Features.nutrition.Commands;
public class CreateFoodItemCommand : UpsertFoodItemRequest, IRequest<FoodItemDto>
{
    [JsonIgnore]
    public Guid SchoolId { get; set; }       // lấy từ token hoặc truyền vào
    [JsonIgnore]
    public Guid? CreatedBy { get; set; }     // user hiện tại
    public List<FoodItemIngredientRequestDto>? Ingredients { get; set; }
}

public class UpdateFoodItemCommand : UpsertFoodItemRequest, IRequest<FoodItemDto>
{
    [JsonIgnore]
    public int FoodId { get; set; }
    public List<FoodItemIngredientRequestDto>? Ingredients { get; set; }
}

public class DeleteFoodItemCommand : IRequest
{
    public int FoodId { get; set; }

    /// <summary>
    /// Default = false: soft delete (IsActive = 0)
    /// True: nếu không còn quan hệ thì hard delete, nếu còn thì throw.
    /// </summary>
    public bool HardDeleteIfNoRelation { get; set; } = false;
}
